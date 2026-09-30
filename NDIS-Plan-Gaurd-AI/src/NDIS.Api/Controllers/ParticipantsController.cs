using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Common.Security;
using NDIS.Application.Participants;
using NDIS.Contracts.Participants;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/participants")]
[Authorize(Roles = AppRoles.ServiceProvider)]
public sealed class ParticipantsController : ControllerBase
{
    private readonly IParticipantService _service;
    public ParticipantsController(IParticipantService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok((await _service.GetMyParticipantsAsync(cancellationToken)).Select(Map));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var participant = await _service.GetByIdAsync(id, cancellationToken);
        return participant is null ? NotFound() : Ok(Map(participant));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateParticipantRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(new CreateParticipantCommand(
            request.NdisNumber, request.FirstName, request.LastName, request.DateOfBirth,
            request.Email, request.PhoneNumber, request.PlanStartDate, request.PlanEndDate,request.EmergencyContactName,
            request.EmergencyContactRelationship,
           request.EmergencyContactPhoneNumber,request.PlanTotalBudget), cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, new { participantId = result.ParticipantId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateParticipantRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, new UpdateParticipantCommand(
            request.FirstName, request.LastName, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.PlanStartDate, request.PlanEndDate,request.EmergencyContactName,
            request.EmergencyContactRelationship,request.EmergencyContactPhoneNumber,
            request.PlanTotalBudget), cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeactivateAsync(id, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }

    private static ParticipantResponse Map(ParticipantDto x) => new(
        x.Id, x.NdisNumber, x.FirstName, x.LastName, x.DateOfBirth, x.Email, x.PhoneNumber,
        x.PlanStartDate, x.PlanEndDate,
        x.EmergencyContactName, x.EmergencyContactRelationship, x.EmergencyContactPhoneNumber, x.PlanTotalBudget, x.Status, x.CreatedAtUtc);
}
