using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Risk;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Risk;

public sealed class ClaimRiskFeatureBuilder : IClaimRiskFeatureBuilder
{
    private const decimal MaximumRatio = 10m;

    private readonly IAppDbContext _dbContext;

    public ClaimRiskFeatureBuilder(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClaimRiskFeatureBuildResult> BuildAsync(
        Guid claimId,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .SingleOrDefaultAsync(x => x.Id == claimId, cancellationToken);

        if (claim is null)
            return ClaimRiskFeatureBuildResult.Failure("Claim was not found for ML risk scoring.");

        if (!claim.SubmittedAtUtc.HasValue)
            return ClaimRiskFeatureBuildResult.Failure("Claim must be submitted before ML risk scoring can run.");

        if (!claim.Participant.PlanTotalBudget.HasValue || claim.Participant.PlanTotalBudget.Value <= 0)
        {
            return ClaimRiskFeatureBuildResult.Failure(
                "Participant plan total budget is required before ML risk scoring can run.");
        }

        var submittedAtUtc = claim.SubmittedAtUtc.Value;

        var participantHistory = await _dbContext.Claims
            .AsNoTracking()
            .Where(x =>
                x.ParticipantId == claim.ParticipantId &&
                x.Id != claim.Id &&
                x.SubmittedAtUtc.HasValue &&
                x.SubmittedAtUtc.Value < submittedAtUtc)
            .Select(x => new HistoricalClaim(x.Amount, x.SubmittedAtUtc!.Value, x.Status))
            .ToListAsync(cancellationToken);

        var providerHistory = await _dbContext.Claims
            .AsNoTracking()
            .Where(x =>
                x.ServiceProviderId == claim.ServiceProviderId &&
                x.Id != claim.Id &&
                x.SubmittedAtUtc.HasValue &&
                x.SubmittedAtUtc.Value < submittedAtUtc)
            .Select(x => new HistoricalClaim(x.Amount, x.SubmittedAtUtc!.Value, x.Status))
            .ToListAsync(cancellationToken);

        var duplicateWarning = await _dbContext.Claims
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id != claim.Id &&
                x.ServiceProviderId == claim.ServiceProviderId &&
                x.ParticipantId == claim.ParticipantId &&
                x.SupportCategory == claim.SupportCategory &&
                x.Amount == claim.Amount &&
                x.ServiceFrom == claim.ServiceFrom &&
                x.ServiceTo == claim.ServiceTo &&
                x.SubmittedAtUtc.HasValue &&
                x.SubmittedAtUtc.Value < submittedAtUtc,
                cancellationToken);

        var planTotalBudget = claim.Participant.PlanTotalBudget.Value;

        var planStartDate = claim.Participant.PlanStartDate?.Date;
        var committedSpendQuery = _dbContext.Claims
            .AsNoTracking()
            .Where(x =>
                x.ParticipantId == claim.ParticipantId &&
                x.Id != claim.Id &&
                x.SubmittedAtUtc.HasValue &&
                x.SubmittedAtUtc.Value < submittedAtUtc &&
                x.Status != ClaimStatus.Draft &&
                x.Status != ClaimStatus.Rejected &&
                x.Status != ClaimStatus.Cancelled);

        if (planStartDate.HasValue)
            committedSpendQuery = committedSpendQuery.Where(x => x.ServiceFrom >= planStartDate.Value);

        var committedSpend = await committedSpendQuery
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var planRemainingBefore = Math.Max(0m, planTotalBudget - committedSpend);
        var planUtilisationBefore = planTotalBudget <= 0
            ? 0m
            : Math.Clamp(committedSpend / planTotalBudget, 0m, 1m);

        var units = claim.Units.GetValueOrDefault(1);
        if (units <= 0)
            units = 1;

        var unitPrice = claim.UnitPrice.GetValueOrDefault(claim.Amount / units);
        if (unitPrice <= 0)
            unitPrice = claim.Amount / units;

        var participantAverage = Average(participantHistory);
        var providerAverage = Average(providerHistory);
        var planLimitWarning = claim.Amount > planRemainingBefore;

        var validationFindingCount =
            (duplicateWarning ? 1 : 0) +
            (planLimitWarning ? 1 : 0);

        var validationHighCount = validationFindingCount;

        var serviceTo = claim.ServiceTo.Date;
        var daysServiceToSubmission = Math.Max(
            0,
            (submittedAtUtc.Date - serviceTo).Days);

        var context = new ClaimRiskContext
        {
            ClaimId = claim.Id.ToString("D"),
            CorrelationId = correlationId,
            ClaimAmount = claim.Amount,
            Units = units,
            UnitPrice = unitPrice,
            ServiceCategory = claim.SupportCategory,
            DaysServiceToSubmission = daysServiceToSubmission,
            PlanTotalBudget = planTotalBudget,
            PlanRemainingBefore = planRemainingBefore,
            PlanUtilisationBefore = planUtilisationBefore,
            ClaimToRemainingRatio = Ratio(claim.Amount, planRemainingBefore, whenMissing: MaximumRatio),
            ParticipantClaims7d = CountWithin(participantHistory, submittedAtUtc, 7),
            ParticipantClaims30d = CountWithin(participantHistory, submittedAtUtc, 30),
            ParticipantClaims90d = CountWithin(participantHistory, submittedAtUtc, 90),
            ParticipantAvgClaimPrior = participantAverage,
            ClaimToParticipantAvg = Ratio(claim.Amount, participantAverage, whenMissing: 1m),
            DaysSinceParticipantPreviousClaim = DaysSincePrevious(participantHistory, submittedAtUtc),
            ProviderClaims7d = CountWithin(providerHistory, submittedAtUtc, 7),
            ProviderClaims30d = CountWithin(providerHistory, submittedAtUtc, 30),
            ProviderClaims90d = CountWithin(providerHistory, submittedAtUtc, 90),
            ProviderAvgClaimPrior = providerAverage,
            ClaimToProviderAvg = Ratio(claim.Amount, providerAverage, whenMissing: 1m),
            ValidationFindingCount = validationFindingCount,
            ValidationHighCount = validationHighCount,
            DuplicateWarning = duplicateWarning,
            PlanLimitWarning = planLimitWarning
        };

        return ClaimRiskFeatureBuildResult.Success(context);
    }

    private static int CountWithin(
        IReadOnlyCollection<HistoricalClaim> claims,
        DateTime submittedAtUtc,
        int days)
    {
        var from = submittedAtUtc.AddDays(-days);
        return claims.Count(x => x.SubmittedAtUtc >= from && x.SubmittedAtUtc < submittedAtUtc);
    }

    private static decimal? Average(IReadOnlyCollection<HistoricalClaim> claims) =>
        claims.Count == 0 ? null : claims.Average(x => x.Amount);

    private static int? DaysSincePrevious(
        IReadOnlyCollection<HistoricalClaim> claims,
        DateTime submittedAtUtc)
    {
        if (claims.Count == 0)
            return null;

        var previous = claims.MaxBy(x => x.SubmittedAtUtc);
        if (previous is null)
            return null;

        return Math.Max(0, (submittedAtUtc.Date - previous.SubmittedAtUtc.Date).Days);
    }

    private static decimal Ratio(decimal numerator, decimal? denominator, decimal whenMissing)
    {
        if (!denominator.HasValue || denominator.Value <= 0)
            return whenMissing;

        return Math.Clamp(numerator / denominator.Value, 0m, MaximumRatio);
    }

    private sealed record HistoricalClaim(
        decimal Amount,
        DateTime SubmittedAtUtc,
        ClaimStatus Status);
}
