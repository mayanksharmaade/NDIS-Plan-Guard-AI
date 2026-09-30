using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;
using NDIS.Domain.Entities;

namespace NDIS.Application.Participants;

public sealed class ParticipantService : IParticipantService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAppDbContext _dbContext;

    public ParticipantService(ICurrentUserService currentUser, IAppDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ParticipantDto>> GetMyParticipantsAsync(CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null) return Array.Empty<ParticipantDto>();

        var participants = await _dbContext.Participants
            .AsNoTracking()
            .Where(x => x.ServiceProviderId == providerId.Value)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

        return participants.Select(Map).ToArray();
    }

    public async Task<ParticipantDto?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null) return null;

        var participant = await _dbContext.Participants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == participantId && x.ServiceProviderId == providerId.Value, cancellationToken);

        return participant is null ? null : Map(participant);
    }

    public async Task<ParticipantOperationResult> CreateAsync(
        CreateParticipantCommand command,
        CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null) return ParticipantOperationResult.Failure("Active Service Provider membership was not found.");

        var ndisNumber = command.NdisNumber.Trim();
        if (string.IsNullOrWhiteSpace(ndisNumber))
            return ParticipantOperationResult.Failure("NDIS number is required.");

        if (command.PlanStartDate.HasValue && command.PlanEndDate.HasValue &&
            command.PlanEndDate.Value.Date < command.PlanStartDate.Value.Date)
            return ParticipantOperationResult.Failure("Plan end date cannot be before plan start date.");

        if (command.PlanTotalBudget.HasValue && command.PlanTotalBudget.Value <= 0)
            return ParticipantOperationResult.Failure("Plan total budget must be greater than zero when supplied.");

        var exists = await _dbContext.Participants.AnyAsync(
            x => x.ServiceProviderId == providerId.Value && x.NdisNumber == ndisNumber,
            cancellationToken);
        if (exists) return ParticipantOperationResult.Failure("Participant already exists for this Service Provider.");

        var participant = new Participant(
            providerId.Value,
            ndisNumber,
            command.FirstName.Trim(),
            command.LastName.Trim(),
            command.DateOfBirth?.Date,
            Clean(command.Email),
            Clean(command.PhoneNumber),
            command.PlanStartDate?.Date,
            command.PlanEndDate?.Date,
            command.EmergencyContactName?.Trim(),
            command.EmergencyContactRelationship?.Trim(),
            command.EmergencyContactPhoneNumber?.Trim(),
            command.PlanTotalBudget);

        _dbContext.Participants.Add(participant);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ParticipantOperationResult.Success(participant.Id);
    }

    public async Task<ParticipantOperationResult> UpdateAsync(
        Guid participantId,
        UpdateParticipantCommand command,
        CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null) return ParticipantOperationResult.Failure("Active Service Provider membership was not found.");

        var participant = await _dbContext.Participants.SingleOrDefaultAsync(
            x => x.Id == participantId && x.ServiceProviderId == providerId.Value,
            cancellationToken);
        if (participant is null) return ParticipantOperationResult.Failure("Participant was not found.");

        if (command.PlanStartDate.HasValue && command.PlanEndDate.HasValue &&
            command.PlanEndDate.Value.Date < command.PlanStartDate.Value.Date)
            return ParticipantOperationResult.Failure("Plan end date cannot be before plan start date.");

        if (command.PlanTotalBudget.HasValue && command.PlanTotalBudget.Value <= 0)
            return ParticipantOperationResult.Failure("Plan total budget must be greater than zero when supplied.");

        participant.Update(
            command.FirstName.Trim(),
            command.LastName.Trim(),
            command.DateOfBirth?.Date,
            Clean(command.Email),
            Clean(command.PhoneNumber),
            command.PlanStartDate?.Date,
            command.PlanEndDate?.Date,
             command.EmergencyContactName?.Trim(),
            command.EmergencyContactRelationship?.Trim(),
            command.EmergencyContactPhoneNumber?.Trim(),
            command.PlanTotalBudget);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ParticipantOperationResult.Success(participant.Id);
    }

    public async Task<ParticipantOperationResult> DeactivateAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        var providerId = await GetProviderIdAsync(cancellationToken);
        if (providerId is null) return ParticipantOperationResult.Failure("Active Service Provider membership was not found.");

        var participant = await _dbContext.Participants.SingleOrDefaultAsync(
            x => x.Id == participantId && x.ServiceProviderId == providerId.Value,
            cancellationToken);
        if (participant is null) return ParticipantOperationResult.Failure("Participant was not found.");

        participant.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ParticipantOperationResult.Success(participant.Id);
    }

    private async Task<Guid?> GetProviderIdAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid identityUserId) return null;
        return await CurrentProviderResolver.ResolveAsync(_dbContext, identityUserId, cancellationToken);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ParticipantDto Map(Participant x) => new(
        x.Id, x.NdisNumber, x.FirstName, x.LastName, x.DateOfBirth, x.Email, x.PhoneNumber,
        x.PlanStartDate, x.PlanEndDate, x.EmergencyContactName, x.EmergencyContactRelationship, x.EmergencyContactPhoneNumber,
        x.PlanTotalBudget, x.Status.ToString(), x.CreatedAtUtc);
}
