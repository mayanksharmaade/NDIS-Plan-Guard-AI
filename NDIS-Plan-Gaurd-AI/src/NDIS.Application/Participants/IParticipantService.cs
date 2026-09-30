namespace NDIS.Application.Participants;

public interface IParticipantService
{
    Task<IReadOnlyCollection<ParticipantDto>> GetMyParticipantsAsync(CancellationToken cancellationToken = default);
    Task<ParticipantDto?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<ParticipantOperationResult> CreateAsync(CreateParticipantCommand command, CancellationToken cancellationToken = default);
    Task<ParticipantOperationResult> UpdateAsync(Guid participantId, UpdateParticipantCommand command, CancellationToken cancellationToken = default);
    Task<ParticipantOperationResult> DeactivateAsync(Guid participantId, CancellationToken cancellationToken = default);
}
