using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Authentication;
using NDIS.Application.Registration;
using NDIS.Contracts.Identity;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IRegistrationService registrationService, IAuthenticationService authenticationService)
    {
        _registrationService = registrationService;
        _authenticationService = authenticationService;
    }

    [HttpPost("register/service-provider")]
    public async Task<IActionResult> RegisterServiceProvider(RegisterServiceProviderRequest request, CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterServiceProviderAsync(
            new RegisterServiceProviderCommand(
                request.Email, request.Password, request.FirstName, request.LastName,
                request.LegalName, request.TradingName, request.Abn,
                request.NdisRegistrationNumber, request.PhoneNumber),
            cancellationToken);

        if (result.Succeeded)
            return StatusCode(StatusCodes.Status201Created, new RegisterServiceProviderResponse(
                result.UserProfileId!.Value,
                result.ServiceProviderId!.Value,
                request.Email.Trim(),
                "Pending",
                "Registration submitted successfully and is awaiting approval."));

        return result.FailureReason switch
        {
            RegistrationFailureReason.Validation or RegistrationFailureReason.IdentityCreationFailed => BadRequest(new { errors = result.Errors }),
            RegistrationFailureReason.DuplicateEmail or RegistrationFailureReason.DuplicateAbn => Conflict(new { errors = result.Errors }),
            _ => Problem("Registration could not be completed.")
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
        if (!result.Succeeded)
        {
            var status = result.FailureReason == LoginFailureReason.InvalidCredentials
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status403Forbidden;
            return StatusCode(status, new { error = result.Error, reason = result.FailureReason.ToString() });
        }

        return Ok(new LoginResponse(
            result.AccessToken!, result.ExpiresAtUtc!.Value, result.UserProfileId!.Value,
            result.Email!, result.FirstName!, result.LastName!, result.Roles));
    }
}
