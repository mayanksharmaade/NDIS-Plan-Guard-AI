using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Administration;
using NDIS.Application.Common.Security;
using NDIS.Contracts.Administration;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/super-admin")]
[Authorize(Roles = AppRoles.SuperAdmin)]
public sealed class SuperAdminController : ControllerBase
{
    private readonly IUserAdministrationService _service;
    public SuperAdminController(IUserAdministrationService service) => _service = service;

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin(CreateAdminRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAdminAsync(
            new CreateAdminCommand(request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber),
            cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, new CreateAdminResponse(result.UserProfileId!.Value, "Admin created successfully."))
            : BadRequest(new { errors = result.Errors });
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<UserSummaryResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _service.GetUsersAsync(cancellationToken);
        return Ok(users.Select(x => new UserSummaryResponse(
            x.UserProfileId, x.IdentityUserId, x.Email, x.FirstName, x.LastName,
            x.UserCategory, x.ApprovalStatus, x.AccountStatus, x.Roles)));
    }

    [HttpPost("users/{userProfileId:guid}/activate")]
    public async Task<IActionResult> Activate(Guid userProfileId, CancellationToken cancellationToken)
    {
        var result = await _service.ActivateUserAsync(userProfileId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("users/{userProfileId:guid}/disable")]
    public async Task<IActionResult> Disable(Guid userProfileId, CancellationToken cancellationToken)
    {
        var result = await _service.DisableUserAsync(userProfileId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }
}
