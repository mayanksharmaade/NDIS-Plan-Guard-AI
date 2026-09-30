using NDIS.Application.ParticipantServices.Models;

namespace NDIS.Application.ParticipantServices;

public interface IParticipantServiceService
{
    Task<IReadOnlyCollection<ParticipantServiceAssignmentDto>> GetAssignmentsAsync(
        Guid participantId,
        CancellationToken cancellationToken = default);

    Task<ParticipantServiceAssignmentDto> AssignEmployeeAsync(
        Guid participantId,
        CreateParticipantServiceAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ServiceDeliveryDto>> GetDeliveriesAsync(
        Guid participantId,
        CancellationToken cancellationToken = default);

    Task<ServiceDeliveryDto> RecordDeliveryAsync(
        Guid participantId,
        CreateServiceDeliveryRequest request,
        CancellationToken cancellationToken = default);
}
