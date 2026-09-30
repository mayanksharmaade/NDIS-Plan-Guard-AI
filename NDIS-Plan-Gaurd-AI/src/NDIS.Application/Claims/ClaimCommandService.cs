using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Auditing;
using NDIS.Application.Common.Security;
using NDIS.Domain.Common;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Claims;

public sealed class ClaimCommandService : IClaimCommandService
{
    private const int SupportCategoryMaxLength = 150;
    private const int DescriptionMaxLength = 1000;

    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditTrailService _auditTrail;
    private readonly IClaimRiskAssessmentService _riskAssessmentService;

    public ClaimCommandService(
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

    public async Task<ClaimOperationResult> CreateAsync(
        CreateClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null)
            return ClaimOperationResult.Failure("Active Service Provider membership was not found.");

        if (command.ServiceDeliveryIds is { Count: > 0 })
            return await CreateFromServiceDeliveriesAsync(providerId.Value, command, cancellationToken);

        // Legacy/manual draft path retained for existing data and compatibility.
        var validationError = Validate(
            command.ServiceFrom,
            command.ServiceTo,
            command.SupportCategory,
            command.Description,
            command.Amount,
            command.Units,
            command.UnitPrice);

        if (validationError is not null)
            return ClaimOperationResult.Failure(validationError);

        var participant = await _dbContext.Participants.SingleOrDefaultAsync(
            x => x.Id == command.ParticipantId && x.ServiceProviderId == providerId.Value,
            cancellationToken);

        if (participant is null)
            return ClaimOperationResult.Failure("Participant was not found for this Service Provider.");

        if (participant.Status != ParticipantStatus.Active)
            return ClaimOperationResult.Failure("Claims can only be created for an active participant.");

        var claimNumber = CreateClaimNumber();
        var claim = new Claim(
            providerId.Value,
            participant.Id,
            claimNumber,
            command.ServiceFrom,
            command.ServiceTo,
            command.SupportCategory,
            command.Description,
            command.Amount,
            command.Units,
            command.UnitPrice);

        _dbContext.Claims.Add(claim);
        AddCreateAudit(providerId.Value, claim);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ClaimOperationResult.Success(claim.Id, claim.ClaimNumber);
    }

    private async Task<ClaimOperationResult> CreateFromServiceDeliveriesAsync(
        Guid providerId,
        CreateClaimCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Description))
            return ClaimOperationResult.Failure("Description is required.");

        if (command.Description.Trim().Length > DescriptionMaxLength)
            return ClaimOperationResult.Failure($"Description cannot exceed {DescriptionMaxLength} characters.");

        var participant = await _dbContext.Participants
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.ParticipantId && x.ServiceProviderId == providerId,
                cancellationToken);

        if (participant is null)
            return ClaimOperationResult.Failure("Participant was not found for this Service Provider.");

        if (participant.Status != ParticipantStatus.Active)
            return ClaimOperationResult.Failure("Claims can only be created for an active participant.");

        var deliveryIds = command.ServiceDeliveryIds!
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (deliveryIds.Length == 0)
            return ClaimOperationResult.Failure("Select at least one recorded service delivery.");

        var deliveries = await _dbContext.ServiceDeliveries
            .Include(x => x.ServiceProviderEmployee)
            .Include(x => x.ParticipantServiceAssignment)
            .Where(x => deliveryIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (deliveries.Count != deliveryIds.Length)
            return ClaimOperationResult.Failure("One or more selected service deliveries were not found.");

        var alreadyClaimed = await _dbContext.ClaimServiceDeliveries
            .AsNoTracking()
            .AnyAsync(x => deliveryIds.Contains(x.ServiceDeliveryId), cancellationToken);

        if (alreadyClaimed)
            return ClaimOperationResult.Failure("One or more selected service deliveries have already been claimed.");

        if (deliveries.Any(x => x.IsCancelled))
            return ClaimOperationResult.Failure("Cancelled service deliveries cannot be claimed.");

        if (deliveries.Any(x => x.ParticipantId != participant.Id))
            return ClaimOperationResult.Failure("All selected service deliveries must belong to the selected participant.");

        if (deliveries.Any(x => x.ServiceProviderEmployee.ServiceProviderId != providerId))
            return ClaimOperationResult.Failure("All selected service deliveries must belong to the current Service Provider.");

        if (deliveries.Any(x =>
                x.ParticipantServiceAssignment is null
                || !x.ParticipantServiceAssignment.IsActive
                || x.ParticipantServiceAssignment.ParticipantId != participant.Id
                || x.ParticipantServiceAssignment.ServiceProviderEmployeeId != x.ServiceProviderEmployeeId
                || !x.ParticipantServiceAssignment.AgreedHourlyRate.HasValue
                || x.ParticipantServiceAssignment.AgreedHourlyRate.Value <= 0))
        {
            return ClaimOperationResult.Failure(
                "Each service delivery must have a valid active participant/employee assignment and agreed billing rate.");
        }

        var employeeId = deliveries[0].ServiceProviderEmployeeId;
        var supportCode = deliveries[0].SupportCategory;
        var agreedRate = deliveries[0].HourlyRate;

        if (deliveries.Any(x => x.ServiceProviderEmployeeId != employeeId))
            return ClaimOperationResult.Failure("Create separate claims for different support workers.");

        if (deliveries.Any(x => x.SupportCategory != supportCode))
            return ClaimOperationResult.Failure("Create separate claims for different support services.");

        if (!SupportServiceCatalog.IsValid(supportCode))
            return ClaimOperationResult.Failure("The selected deliveries contain an invalid support service code.");

        if (deliveries.Any(x => x.HourlyRate != agreedRate))
            return ClaimOperationResult.Failure("Create separate claims when the agreed participant billing rate changes.");

        var totalHours = Math.Round(deliveries.Sum(x => x.ServiceHours), 2);
        var amount = Math.Round(deliveries.Sum(x => x.Amount), 2);
        var serviceFrom = deliveries.Min(x => x.ServiceStartUtc.UtcDateTime).Date;
        var serviceTo = deliveries.Max(x => x.ServiceEndUtc.UtcDateTime).Date;

        // The period fields are used as a filter in the UI. Do not allow selected
        // delivery evidence outside the provider-selected period.
        if (serviceFrom < command.ServiceFrom.Date || serviceTo > command.ServiceTo.Date)
            return ClaimOperationResult.Failure("Selected service deliveries must fall inside the selected claim period.");

        var claimNumber = CreateClaimNumber();
        var claim = new Claim(
            providerId,
            participant.Id,
            claimNumber,
            serviceFrom,
            serviceTo,
            supportCode,
            command.Description,
            amount,
            units: null,
            unitPrice: null,
            totalServiceHours: totalHours,
            agreedHourlyRate: agreedRate)
        {
            ServiceProviderEmployeeId = employeeId,
            ServiceDeliveryId = deliveries.Count == 1 ? deliveries[0].Id : null
        };

        _dbContext.Claims.Add(claim);

        foreach (var delivery in deliveries)
        {
            _dbContext.ClaimServiceDeliveries.Add(new ClaimServiceDelivery
            {
                ClaimId = claim.Id,
                ServiceDeliveryId = delivery.Id
            });
        }

        AddCreateAudit(providerId, claim);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ClaimOperationResult.Success(claim.Id, claim.ClaimNumber);
    }

    public async Task<ClaimOperationResult> UpdateAsync(
        Guid claimId,
        UpdateClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null)
            return ClaimOperationResult.Failure("Active Service Provider membership was not found.");

        var claim = await _dbContext.Claims.SingleOrDefaultAsync(
            x => x.Id == claimId && x.ServiceProviderId == providerId.Value,
            cancellationToken);

        if (claim is null)
            return ClaimOperationResult.Failure("Claim was not found.");

        var deliveryBased = await _dbContext.ClaimServiceDeliveries
            .AsNoTracking()
            .AnyAsync(x => x.ClaimId == claimId, cancellationToken);

        if (deliveryBased)
        {
            return ClaimOperationResult.Failure(
                "Delivery-based claim amounts, hours and service details are calculated from recorded service deliveries and cannot be manually edited.");
        }

        var validationError = Validate(
            command.ServiceFrom,
            command.ServiceTo,
            command.SupportCategory,
            command.Description,
            command.Amount,
            command.Units,
            command.UnitPrice);

        if (validationError is not null)
            return ClaimOperationResult.Failure(validationError);

        try
        {
            claim.UpdateDraft(
                command.ServiceFrom,
                command.ServiceTo,
                command.SupportCategory,
                command.Description,
                command.Amount,
                command.Units,
                command.UnitPrice);
        }
        catch (InvalidOperationException ex)
        {
            return ClaimOperationResult.Failure(ex.Message);
        }

        _auditTrail.Add(new AddAuditEventCommand(
            providerId.Value,
            "Claim",
            claim.Id,
            AuditActionType.ClaimUpdated,
            $"Claim {claim.ClaimNumber} was updated."));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ClaimOperationResult.Success(claim.Id, claim.ClaimNumber);
    }

    public async Task<ClaimOperationResult> SubmitAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null)
            return ClaimOperationResult.Failure("Active Service Provider membership was not found.");

        var claim = await _dbContext.Claims.SingleOrDefaultAsync(
            x => x.Id == claimId && x.ServiceProviderId == providerId.Value,
            cancellationToken);

        if (claim is null)
            return ClaimOperationResult.Failure("Claim was not found.");

        try
        {
            claim.Submit();
        }
        catch (InvalidOperationException ex)
        {
            return ClaimOperationResult.Failure(ex.Message);
        }

        _auditTrail.Add(new AddAuditEventCommand(
            providerId.Value,
            "Claim",
            claim.Id,
            AuditActionType.ClaimSubmitted,
            $"Claim {claim.ClaimNumber} was submitted for review."));

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Risk scoring is decision support only. An unavailable ML score does
        // not block a valid claim submission.
        await _riskAssessmentService.ScoreAndPersistAsync(
            claim.Id,
            cancellationToken: cancellationToken);

        return ClaimOperationResult.Success(claim.Id, claim.ClaimNumber);
    }

    private void AddCreateAudit(Guid providerId, Claim claim)
    {
        _auditTrail.Add(new AddAuditEventCommand(
            providerId,
            "Claim",
            claim.Id,
            AuditActionType.ClaimCreated,
            $"Claim {claim.ClaimNumber} was created as a draft."));
    }

    private async Task<Guid?> GetProviderIdAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid identityUserId)
            return null;

        return await CurrentProviderResolver.ResolveAsync(
            _dbContext,
            identityUserId,
            cancellationToken);
    }

    private static string? Validate(
        DateTime serviceFrom,
        DateTime serviceTo,
        string supportCategory,
        string description,
        decimal amount,
        int? units,
        decimal? unitPrice)
    {
        if (serviceTo.Date < serviceFrom.Date)
            return "Service-to date cannot be before service-from date.";

        if (string.IsNullOrWhiteSpace(supportCategory))
            return "Support category is required.";

        if (supportCategory.Trim().Length > SupportCategoryMaxLength)
            return $"Support category cannot exceed {SupportCategoryMaxLength} characters.";

        if (string.IsNullOrWhiteSpace(description))
            return "Description is required.";

        if (description.Trim().Length > DescriptionMaxLength)
            return $"Description cannot exceed {DescriptionMaxLength} characters.";

        if (amount <= 0)
            return "Claim amount must be greater than zero.";

        if (units.HasValue && units.Value <= 0)
            return "Claim units must be greater than zero when supplied.";

        if (unitPrice.HasValue && !units.HasValue)
            return "Claim units are required when unit price is supplied.";

        if (unitPrice.HasValue && unitPrice.Value <= 0)
            return "Claim unit price must be greater than zero when supplied.";

        return null;
    }

    private static string CreateClaimNumber() =>
        $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant();
}
