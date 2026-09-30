using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Common.Security;
using NDIS.Application.Providers;
using NDIS.Contracts.Providers;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/service-providers")]
[Authorize(Roles = AppRoles.ServiceProvider)]
public sealed class ServiceProvidersController : ControllerBase
{
    private readonly IServiceProviderProfileService _service;
    public ServiceProvidersController(IServiceProviderProfileService service) => _service = service;

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var x = await _service.GetMyProfileAsync(cancellationToken);
        return x is null ? NotFound() : Ok(new ServiceProviderProfileResponse(
            x.ServiceProviderId, x.UserProfileId, x.Email, x.FirstName, x.LastName,
            x.LegalName, x.TradingName, x.Abn, x.NdisRegistrationNumber, x.ApprovalStatus, x.Status));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMine(UpdateServiceProviderProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateMyProfileAsync(new UpdateServiceProviderProfileCommand(
            request.FirstName, request.LastName, request.LegalName, request.TradingName,
            request.Abn, request.NdisRegistrationNumber), cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { error = result.Error });
    }
}
