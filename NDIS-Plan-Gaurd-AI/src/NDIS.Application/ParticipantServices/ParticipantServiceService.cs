using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;
using NDIS.Application.ParticipantServices.Models;
using NDIS.Domain.Common;
using NDIS.Domain.Entities;

namespace NDIS.Application.ParticipantServices;

public sealed class ParticipantServiceService : IParticipantServiceService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ParticipantServiceService(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ParticipantServiceAssignmentDto>> GetAssignmentsAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        var currentProviderId = await GetCurrentProviderIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active Service Provider membership was not found.");

        var participantBelongsToProvider = await _db.Participants.AsNoTracking().AnyAsync(
            x => x.Id == participantId && x.ServiceProviderId == currentProviderId,
            cancellationToken);

        if (!participantBelongsToProvider)
            throw new InvalidOperationException("Participant was not found for this Service Provider.");

        return await _db.ParticipantServiceAssignments
            .AsNoTracking()
            .Where(x => x.ParticipantId == participantId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new ParticipantServiceAssignmentDto(
                x.Id,
                x.ParticipantId,
                x.ServiceProviderEmployeeId,
                x.ServiceProviderEmployee.FirstName + " " + x.ServiceProviderEmployee.LastName,
                x.ServiceProviderEmployee.ServiceProvider.TradingName
                    ?? x.ServiceProviderEmployee.ServiceProvider.LegalName,
                x.SupportCategory,
                x.StartDate,
                x.EndDate,
                x.ServiceProviderEmployee.DefaultHourlyRate,
                x.AgreedHourlyRate,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<ParticipantServiceAssignmentDto> AssignEmployeeAsync(
        Guid participantId,
        CreateParticipantServiceAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            throw new InvalidOperationException("End date cannot be before start date.");

        if (!SupportServiceCatalog.IsValid(request.SupportCategory))
            throw new InvalidOperationException("Select a valid support service.");

        if (!request.AgreedHourlyRate.HasValue || request.AgreedHourlyRate.Value <= 0)
            throw new InvalidOperationException("Agreed participant billing rate is required and must be greater than zero.");

        var currentProviderId = await GetCurrentProviderIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active Service Provider membership was not found.");

        var participant = await _db.Participants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == participantId, cancellationToken)
            ?? throw new InvalidOperationException("Participant not found.");

        var employee = await _db.ServiceProviderEmployees
            .Include(x => x.ServiceProvider)
            .SingleOrDefaultAsync(
                x => x.Id == request.ServiceProviderEmployeeId && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("Active provider employee not found.");

        if (participant.ServiceProviderId != currentProviderId || employee.ServiceProviderId != currentProviderId)
        {
            throw new InvalidOperationException(
                "The participant and employee must belong to the same Service Provider.");
        }

        if (!employee.DefaultHourlyRate.HasValue || employee.DefaultHourlyRate.Value <= 0)
        {
            throw new InvalidOperationException(
                "The employee must have an employee pay rate before being assigned to a participant.");
        }

        var supportCode = SupportServiceCatalog.Normalize(request.SupportCategory);

        var overlappingAssignment = await _db.ParticipantServiceAssignments.AnyAsync(
            x => x.ParticipantId == participantId
                 && x.ServiceProviderEmployeeId == employee.Id
                 && x.SupportCategory == supportCode
                 && x.IsActive
                 && (!x.EndDate.HasValue || x.EndDate.Value >= request.StartDate)
                 && (!request.EndDate.HasValue || x.StartDate <= request.EndDate.Value),
            cancellationToken);

        if (overlappingAssignment)
            throw new InvalidOperationException("This employee already has an overlapping active assignment for this participant and service.");

        var entity = new ParticipantServiceAssignment
        {
            ParticipantId = participantId,
            ServiceProviderEmployeeId = employee.Id,
            SupportCategory = supportCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AgreedHourlyRate = request.AgreedHourlyRate.Value
        };

        _db.ParticipantServiceAssignments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var providerName = employee.ServiceProvider.TradingName
            ?? employee.ServiceProvider.LegalName;

        return new ParticipantServiceAssignmentDto(
            entity.Id,
            participantId,
            employee.Id,
            $"{employee.FirstName} {employee.LastName}",
            providerName,
            entity.SupportCategory,
            entity.StartDate,
            entity.EndDate,
            employee.DefaultHourlyRate,
            entity.AgreedHourlyRate,
            entity.IsActive);
    }

    public async Task<IReadOnlyCollection<ServiceDeliveryDto>> GetDeliveriesAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        var currentProviderId = await GetCurrentProviderIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active Service Provider membership was not found.");

        var participantBelongsToProvider = await _db.Participants.AsNoTracking().AnyAsync(
            x => x.Id == participantId && x.ServiceProviderId == currentProviderId,
            cancellationToken);

        if (!participantBelongsToProvider)
            throw new InvalidOperationException("Participant was not found for this Service Provider.");

        return await _db.ServiceDeliveries
            .AsNoTracking()
            .Where(x => x.ParticipantId == participantId && !x.IsCancelled)
            .OrderByDescending(x => x.ServiceStartUtc)
            .Select(x => new ServiceDeliveryDto(
                x.Id,
                x.ParticipantId,
                x.ServiceProviderEmployeeId,
                x.ParticipantServiceAssignmentId,
                x.ServiceProviderEmployee.FirstName + " " + x.ServiceProviderEmployee.LastName,
                x.ServiceProviderEmployee.ServiceProvider.TradingName
                    ?? x.ServiceProviderEmployee.ServiceProvider.LegalName,
                x.SupportCategory,
                x.ServiceStartUtc,
                x.ServiceEndUtc,
                x.ServiceHours,
                x.HourlyRate,
                x.Amount,
                x.ServiceLocation,
                x.Notes,
                x.ClaimLinks.Any()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceDeliveryDto> RecordDeliveryAsync(
        Guid participantId,
        CreateServiceDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ServiceEndUtc <= request.ServiceStartUtc)
            throw new InvalidOperationException("Service end must be after service start.");

        var currentProviderId = await GetCurrentProviderIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active Service Provider membership was not found.");

        var participant = await _db.Participants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == participantId, cancellationToken)
            ?? throw new InvalidOperationException("Participant not found.");

        var assignment = await _db.ParticipantServiceAssignments
            .Include(x => x.ServiceProviderEmployee)
                .ThenInclude(x => x.ServiceProvider)
            .SingleOrDefaultAsync(
                x => x.Id == request.ParticipantServiceAssignmentId
                     && x.ParticipantId == participantId
                     && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("Active participant service assignment not found.");

        var employee = assignment.ServiceProviderEmployee;

        if (!employee.IsActive)
            throw new InvalidOperationException("The assigned employee is inactive.");

        if (participant.ServiceProviderId != currentProviderId || employee.ServiceProviderId != currentProviderId)
            throw new InvalidOperationException("The participant and assigned employee must belong to the current Service Provider.");

        if (!employee.DefaultHourlyRate.HasValue || employee.DefaultHourlyRate.Value <= 0)
            throw new InvalidOperationException("The employee does not have a valid employee pay rate.");

        if (!assignment.AgreedHourlyRate.HasValue || assignment.AgreedHourlyRate.Value <= 0)
            throw new InvalidOperationException("The participant assignment does not have a valid agreed billing rate.");

        var serviceDate = DateOnly.FromDateTime(request.ServiceStartUtc.UtcDateTime.Date);
        var serviceEndDate = DateOnly.FromDateTime(request.ServiceEndUtc.UtcDateTime.Date);

        if (serviceDate < assignment.StartDate
            || (assignment.EndDate.HasValue && serviceEndDate > assignment.EndDate.Value))
        {
            throw new InvalidOperationException("Service delivery must fall within the employee assignment dates.");
        }

        var overlaps = await _db.ServiceDeliveries.AnyAsync(
            x => x.ServiceProviderEmployeeId == employee.Id
                 && !x.IsCancelled
                 && x.ServiceStartUtc < request.ServiceEndUtc
                 && x.ServiceEndUtc > request.ServiceStartUtc,
            cancellationToken);

        if (overlaps)
            throw new InvalidOperationException("This employee already has an overlapping service delivery.");

        var hours = Math.Round(
            (decimal)(request.ServiceEndUtc - request.ServiceStartUtc).TotalHours,
            2);

        if (hours <= 0)
            throw new InvalidOperationException("Service hours must be greater than zero.");

        var billingRate = assignment.AgreedHourlyRate.Value;
        var amount = Math.Round(hours * billingRate, 2);

        var entity = new ServiceDelivery
        {
            ParticipantId = participantId,
            ServiceProviderEmployeeId = employee.Id,
            ParticipantServiceAssignmentId = assignment.Id,
            SupportCategory = assignment.SupportCategory,
            ServiceStartUtc = request.ServiceStartUtc,
            ServiceEndUtc = request.ServiceEndUtc,
            ServiceHours = hours,
            HourlyRate = billingRate,
            Amount = amount,
            ServiceLocation = request.ServiceLocation?.Trim(),
            Notes = request.Notes?.Trim()
        };

        _db.ServiceDeliveries.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var providerName = employee.ServiceProvider.TradingName
            ?? employee.ServiceProvider.LegalName;

        return new ServiceDeliveryDto(
            entity.Id,
            participantId,
            employee.Id,
            assignment.Id,
            $"{employee.FirstName} {employee.LastName}",
            providerName,
            entity.SupportCategory,
            entity.ServiceStartUtc,
            entity.ServiceEndUtc,
            entity.ServiceHours,
            entity.HourlyRate,
            entity.Amount,
            entity.ServiceLocation,
            entity.Notes,
            false);
    }

    private async Task<Guid?> GetCurrentProviderIdAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid identityUserId)
            return null;

        return await CurrentProviderResolver.ResolveAsync(
            _db,
            identityUserId,
            cancellationToken);
    }
}
