using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Auditing;
using NDIS.Application.Common.Security;
using NDIS.Contracts.Auditing;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = AppRoles.ClaimReaders)]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditTrailService _service;

    public AuditController(IAuditTrailService service) => _service = service;

    [HttpGet("events")]
    public async Task<IActionResult> Events(
        [FromQuery] int take = 100,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await _service.GetEventsAsync(
            take,
            entityType,
            entityId,
            cancellationToken);

        return Ok(rows.Select(x => new AuditEventResponse(
            x.Id,
            x.ServiceProviderId,
            x.EntityType,
            x.EntityId,
            x.Action,
            x.Description,
            x.ActorIdentityUserId,
            x.ActorEmail,
            x.OccurredAtUtc)));
    }
}
