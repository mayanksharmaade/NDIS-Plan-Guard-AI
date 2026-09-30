using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Administration;
using NDIS.Application.Common.Security;
using NDIS.Contracts.Administration;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IUserAdministrationService _service;
    public AdminUsersController(IUserAdministrationService service) => _service = service;

    [HttpGet("registrations/pending")]
    public async Task<ActionResult<IReadOnlyCollection<PendingRegistrationResponse>>> Pending(CancellationToken cancellationToken)
    {
        var rows = await _service.GetPendingRegistrationsAsync(cancellationToken);
        return Ok(rows.Select(x => new PendingRegistrationResponse(
            x.UserProfileId, x.ServiceProviderId, x.Email, x.FirstName, x.LastName,
            x.LegalName, x.TradingName, x.Abn, x.NdisRegistrationNumber, x.RegisteredAtUtc)));
    }

    [HttpPost("registrations/{userProfileId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid userProfileId, CancellationToken cancellationToken)
    {
        var result = await _service.ApproveRegistrationAsync(userProfileId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("registrations/{userProfileId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid userProfileId, CancellationToken cancellationToken)
    {
        var result = await _service.RejectRegistrationAsync(userProfileId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }
}
