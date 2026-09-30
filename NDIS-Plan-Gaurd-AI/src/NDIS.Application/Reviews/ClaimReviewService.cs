using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Auditing;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Reviews;

public sealed class ClaimReviewService : IClaimReviewService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditTrailService _auditTrail;
    private readonly IClaimRiskAssessmentService _riskAssessmentService;

    public ClaimReviewService(
        IAppDbContext dbContext,
        ICurrentUserService currentUser,
        IAuditTrailService auditTrail,
        IClaimRiskAssessmentService riskAssessmentService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _auditTrail = auditTrail;
        _riskAssessmentService = riskAssessmentService;
    }

    public async Task<IReadOnlyCollection<ReviewQueueItemDto>> GetQueueAsync(
        CancellationToken cancellationToken = default)
    {
        var statuses = new[]
        {
            ClaimStatus.Submitted,
            ClaimStatus.ReviewRequired,
            ClaimStatus.Processing
        };

        var rows = await _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .Include(x => x.ServiceProvider)
            .Where(x => statuses.Contains(x.Status))
            .OrderBy(x => x.Status == ClaimStatus.Processing ? 0 : 1)
            .ThenBy(x => x.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        var claimIds = rows.Select(x => x.Id).ToArray();

        var assessments = await _dbContext.ClaimRiskAssessments
            .AsNoTracking()
            .Where(x => claimIds.Contains(x.ClaimId))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var latestRiskByClaim = assessments
            .GroupBy(x => x.ClaimId)
            .ToDictionary(x => x.Key, x => x.First());

        return rows.Select(x =>
        {
            latestRiskByClaim.TryGetValue(x.Id, out var risk);

            return new ReviewQueueItemDto(
                x.Id,
                x.ClaimNumber,
                x.ServiceProviderId,
                ProviderName(x.ServiceProvider),
                x.ParticipantId,
                ParticipantName(x.Participant),
                x.Participant.NdisNumber,
                x.ServiceFrom,
                x.ServiceTo,
                x.SupportCategory,
                x.Amount,
                x.Status.ToString(),
                x.SubmittedAtUtc,
                x.ReviewStartedAtUtc,
                risk?.Available ?? false,
                risk?.Score,
                risk?.RiskBand);
        })
        .OrderByDescending(x => x.RiskScore ?? -1)
        .ThenBy(x => x.SubmittedAtUtc)
        .ToArray();
    }

    public async Task<ClaimForReviewDto?> GetClaimAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .Include(x => x.ServiceProvider)
            .SingleOrDefaultAsync(x => x.Id == claimId, cancellationToken);

        if (claim is null)
            return null;

        var reviews = await _dbContext.ClaimReviews
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId)
            .OrderByDescending(x => x.ReviewedAtUtc)
            .Select(x => new ClaimReviewHistoryItemDto(
                x.Id,
                x.ReviewerIdentityUserId,
                x.ReviewerEmail,
                x.Outcome.ToString(),
                x.Comments,
                x.ReviewedAtUtc))
            .ToListAsync(cancellationToken);

        var risk = await _riskAssessmentService.GetLatestAsync(claim.Id, cancellationToken);

        return new ClaimForReviewDto(
            claim.Id,
            claim.ClaimNumber,
            claim.ServiceProviderId,
            ProviderName(claim.ServiceProvider),
            claim.ParticipantId,
            ParticipantName(claim.Participant),
            claim.Participant.NdisNumber,
            claim.ServiceFrom,
            claim.ServiceTo,
            claim.SupportCategory,
            claim.Description,
            claim.Amount,
            claim.Status.ToString(),
            claim.CreatedAtUtc,
            claim.SubmittedAtUtc,
            claim.ReviewStartedAtUtc,
            claim.DecidedAtUtc,
            reviews,
            risk);
    }

    public async Task<ClaimReviewOperationResult> StartReviewAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims.SingleOrDefaultAsync(
            x => x.Id == claimId,
            cancellationToken);

        if (claim is null)
            return ClaimReviewOperationResult.Failure("Claim was not found.");

        if (claim.Status == ClaimStatus.Processing)
            return ClaimReviewOperationResult.Success();

        try
        {
            claim.StartReview();
        }
        catch (InvalidOperationException ex)
        {
            return ClaimReviewOperationResult.Failure(ex.Message);
        }

        _auditTrail.Add(new AddAuditEventCommand(
            claim.ServiceProviderId,
            "Claim",
            claim.Id,
            AuditActionType.ClaimReviewStarted,
            $"Review started for claim {claim.ClaimNumber}."));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ClaimReviewOperationResult.Success();
    }

    public async Task<ClaimReviewOperationResult> RecordDecisionAsync(
        Guid claimId,
        ReviewDecisionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not Guid reviewerId || string.IsNullOrWhiteSpace(_currentUser.Email))
            return ClaimReviewOperationResult.Failure("Authenticated reviewer identity was not available.");

        if (!TryParseOutcome(command.Decision, out var outcome))
            return ClaimReviewOperationResult.Failure("Decision must be Approved, Rejected, or MoreInformationRequired.");

        if ((outcome is ClaimReviewOutcome.Rejected or ClaimReviewOutcome.MoreInformationRequired) &&
            string.IsNullOrWhiteSpace(command.Comments))
        {
            return ClaimReviewOperationResult.Failure("Comments are required when rejecting a claim or requesting more information.");
        }

        if (command.Comments?.Trim().Length > 1000)
            return ClaimReviewOperationResult.Failure("Review comments cannot exceed 1000 characters.");

        var claim = await _dbContext.Claims.SingleOrDefaultAsync(
            x => x.Id == claimId,
            cancellationToken);

        if (claim is null)
            return ClaimReviewOperationResult.Failure("Claim was not found.");

        try
        {
            claim.ApplyReviewDecision(outcome);
        }
        catch (InvalidOperationException ex)
        {
            return ClaimReviewOperationResult.Failure(ex.Message);
        }

        var review = new ClaimReview(
            claim.Id,
            reviewerId,
            _currentUser.Email,
            outcome,
            command.Comments);

        _dbContext.ClaimReviews.Add(review);
        _auditTrail.Add(new AddAuditEventCommand(
            claim.ServiceProviderId,
            "Claim",
            claim.Id,
            AuditAction(outcome),
            AuditDescription(claim.ClaimNumber, outcome)));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ClaimReviewOperationResult.Success();
    }

    private static bool TryParseOutcome(string value, out ClaimReviewOutcome outcome)
    {
        if (Enum.TryParse(value?.Trim(), ignoreCase: true, out outcome) &&
            Enum.IsDefined(outcome))
        {
            return true;
        }

        outcome = default;
        return false;
    }

    private static AuditActionType AuditAction(ClaimReviewOutcome outcome) => outcome switch
    {
        ClaimReviewOutcome.Approved => AuditActionType.ClaimApproved,
        ClaimReviewOutcome.Rejected => AuditActionType.ClaimRejected,
        ClaimReviewOutcome.MoreInformationRequired => AuditActionType.ClaimMoreInformationRequested,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };

    private static string AuditDescription(string claimNumber, ClaimReviewOutcome outcome) => outcome switch
    {
        ClaimReviewOutcome.Approved => $"Claim {claimNumber} was approved.",
        ClaimReviewOutcome.Rejected => $"Claim {claimNumber} was rejected.",
        ClaimReviewOutcome.MoreInformationRequired => $"More information was requested for claim {claimNumber}.",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };

    private static string ProviderName(ServiceProvider provider) =>
        string.IsNullOrWhiteSpace(provider.TradingName) ? provider.LegalName : provider.TradingName;

    private static string ParticipantName(Participant participant) =>
        $"{participant.FirstName} {participant.LastName}".Trim();
}
