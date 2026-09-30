using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;

namespace NDIS.Application.Claims;

public sealed class ClaimQueryService : IClaimQueryService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAppDbContext _dbContext;
    private readonly IClaimRiskAssessmentService _riskAssessmentService;

    public ClaimQueryService(
        ICurrentUserService currentUser,
        IAppDbContext dbContext,
        IClaimRiskAssessmentService riskAssessmentService)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _riskAssessmentService = riskAssessmentService;
    }

    public async Task<IReadOnlyCollection<ClaimListItemDto>> GetClaimsAsync(
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .AsQueryable();

        if (_currentUser.IsInRole(AppRoles.ServiceProvider))
        {
            if (_currentUser.UserId is not Guid identityUserId)
                return Array.Empty<ClaimListItemDto>();

            var providerId = await CurrentProviderResolver.ResolveAsync(
                _dbContext,
                identityUserId,
                cancellationToken);

            if (providerId is null)
                return Array.Empty<ClaimListItemDto>();

            query = query.Where(x => x.ServiceProviderId == providerId.Value);
        }

        var claims = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return claims.Select(x => new ClaimListItemDto(
            x.Id,
            x.ClaimNumber,
            x.ParticipantId,
            $"{x.Participant.FirstName} {x.Participant.LastName}".Trim(),
            x.ServiceFrom,
            x.ServiceTo,
            x.SupportCategory,
            x.Amount,
            x.Units,
            x.UnitPrice,
            x.Status.ToString(),
            x.CreatedAtUtc,
            x.SubmittedAtUtc)).ToArray();
    }

    public async Task<ClaimDetailsDto?> GetByIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .Where(x => x.Id == claimId);

        if (_currentUser.IsInRole(AppRoles.ServiceProvider))
        {
            if (_currentUser.UserId is not Guid identityUserId)
                return null;

            var providerId = await CurrentProviderResolver.ResolveAsync(
                _dbContext,
                identityUserId,
                cancellationToken);

            if (providerId is null)
                return null;

            query = query.Where(x => x.ServiceProviderId == providerId.Value);
        }

        var x = await query.SingleOrDefaultAsync(cancellationToken);
        if (x is null)
            return null;

        var risk = await _riskAssessmentService.GetLatestAsync(x.Id, cancellationToken);

        var deliveries = await _dbContext.ClaimServiceDeliveries
            .AsNoTracking()
            .Where(link => link.ClaimId == x.Id)
            .OrderBy(link => link.ServiceDelivery.ServiceStartUtc)
            .Select(link => new ClaimServiceDeliveryDto(
                link.ServiceDelivery.Id,
                link.ServiceDelivery.ServiceProviderEmployeeId,
                link.ServiceDelivery.ServiceProviderEmployee.FirstName + " "
                    + link.ServiceDelivery.ServiceProviderEmployee.LastName,
                link.ServiceDelivery.SupportCategory,
                link.ServiceDelivery.ServiceStartUtc,
                link.ServiceDelivery.ServiceEndUtc,
                link.ServiceDelivery.ServiceHours,
                link.ServiceDelivery.HourlyRate,
                link.ServiceDelivery.Amount,
                link.ServiceDelivery.ServiceLocation))
            .ToListAsync(cancellationToken);

        var employeeName = deliveries
            .Select(d => d.EmployeeName)
            .Distinct()
            .Count() == 1
                ? deliveries.FirstOrDefault()?.EmployeeName
                : deliveries.Count > 1 ? "Multiple support workers" : null;

        return new ClaimDetailsDto(
            x.Id,
            x.ClaimNumber,
            x.ServiceProviderId,
            x.ParticipantId,
            x.Participant.NdisNumber,
            $"{x.Participant.FirstName} {x.Participant.LastName}".Trim(),
            x.ServiceFrom,
            x.ServiceTo,
            x.SupportCategory,
            x.Description,
            x.Amount,
            x.Units,
            x.UnitPrice,
            x.Status.ToString(),
            x.CreatedAtUtc,
            x.SubmittedAtUtc,
            risk,
            x.TotalServiceHours,
            x.AgreedHourlyRate,
            employeeName,
            deliveries);
    }

    public async Task<IReadOnlyCollection<EligibleServiceDeliveryDto>> GetEligibleServiceDeliveriesAsync(
        Guid participantId,
        DateTime serviceFrom,
        DateTime serviceTo,
        CancellationToken cancellationToken = default)
    {
        if (serviceTo.Date < serviceFrom.Date)
            return Array.Empty<EligibleServiceDeliveryDto>();

        if (!_currentUser.IsInRole(AppRoles.ServiceProvider)
            || _currentUser.UserId is not Guid identityUserId)
        {
            return Array.Empty<EligibleServiceDeliveryDto>();
        }

        var providerId = await CurrentProviderResolver.ResolveAsync(
            _dbContext,
            identityUserId,
            cancellationToken);

        if (providerId is null)
            return Array.Empty<EligibleServiceDeliveryDto>();

        var participantBelongsToProvider = await _dbContext.Participants
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == participantId && x.ServiceProviderId == providerId.Value,
                cancellationToken);

        if (!participantBelongsToProvider)
            return Array.Empty<EligibleServiceDeliveryDto>();

        var startUtc = new DateTimeOffset(
            DateTime.SpecifyKind(serviceFrom.Date, DateTimeKind.Utc));
        var endExclusiveUtc = new DateTimeOffset(
            DateTime.SpecifyKind(serviceTo.Date.AddDays(1), DateTimeKind.Utc));

        return await _dbContext.ServiceDeliveries
            .AsNoTracking()
            .Where(x =>
                x.ParticipantId == participantId
                && x.ServiceProviderEmployee.ServiceProviderId == providerId.Value
                && !x.IsCancelled
                && x.ParticipantServiceAssignmentId.HasValue
                && x.ParticipantServiceAssignment!.IsActive
                && x.ParticipantServiceAssignment.AgreedHourlyRate.HasValue
                && x.ParticipantServiceAssignment.AgreedHourlyRate.Value > 0
                && x.ServiceStartUtc >= startUtc
                && x.ServiceStartUtc < endExclusiveUtc
                && !x.ClaimLinks.Any())
            .OrderBy(x => x.ServiceStartUtc)
            .Select(x => new EligibleServiceDeliveryDto(
                x.Id,
                x.ParticipantServiceAssignmentId!.Value,
                x.ServiceProviderEmployeeId,
                x.ServiceProviderEmployee.FirstName + " " + x.ServiceProviderEmployee.LastName,
                x.SupportCategory,
                x.ServiceStartUtc,
                x.ServiceEndUtc,
                x.ServiceHours,
                x.HourlyRate,
                x.Amount,
                x.ServiceLocation))
            .ToListAsync(cancellationToken);
    }
}
