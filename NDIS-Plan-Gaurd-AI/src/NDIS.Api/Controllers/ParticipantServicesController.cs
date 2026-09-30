using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Common.Security;
using NDIS.Application.ParticipantServices;
using NDIS.Application.ParticipantServices.Models;

namespace NDIS.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.ServiceProvider)]
[Route("api/participants/{participantId:guid}")]
public sealed class ParticipantServicesController : ControllerBase
{
    private readonly IParticipantServiceService _service;

    public ParticipantServicesController(IParticipantServiceService service)
        => _service = service;

    [HttpGet("service-assignments")]
    public async Task<ActionResult<IReadOnlyCollection<ParticipantServiceAssignmentDto>>> GetAssignments(
        Guid participantId,
        CancellationToken cancellationToken)
        => Ok(await _service.GetAssignmentsAsync(participantId, cancellationToken));

    [HttpPost("service-assignments")]
    public async Task<ActionResult<ParticipantServiceAssignmentDto>> AssignEmployee(
        Guid participantId,
        CreateParticipantServiceAssignmentRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.AssignEmployeeAsync(
            participantId,
            request,
            cancellationToken));

    [HttpGet("service-deliveries")]
    public async Task<ActionResult<IReadOnlyCollection<ServiceDeliveryDto>>> GetDeliveries(
        Guid participantId,
        CancellationToken cancellationToken)
        => Ok(await _service.GetDeliveriesAsync(participantId, cancellationToken));

    [HttpPost("service-deliveries")]
    public async Task<ActionResult<ServiceDeliveryDto>> RecordDelivery(
        Guid participantId,
        CreateServiceDeliveryRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.RecordDeliveryAsync(
            participantId,
            request,
            cancellationToken));
}
