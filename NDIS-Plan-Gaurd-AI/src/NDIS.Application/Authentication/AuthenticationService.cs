using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Identity;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Authentication;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityService _identityService;
    private readonly IAppDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(
        IIdentityService identityService,
        IAppDbContext dbContext,
        IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var identityUser = await _identityService.FindByEmailAsync(email, cancellationToken);

        if (identityUser is null ||
            !await _identityService.CheckPasswordAsync(identityUser.Id, command.Password, cancellationToken))
        {
            return LoginResult.Failure(LoginFailureReason.InvalidCredentials, "Invalid email or password.");
        }

        var profile = await _dbContext.UserProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdentityUserId == identityUser.Id, cancellationToken);

        if (profile is null)
            return LoginResult.Failure(LoginFailureReason.AccountInactive, "User profile was not found.");

        if (profile.ApprovalStatus == ApprovalStatus.Pending)
            return LoginResult.Failure(LoginFailureReason.PendingApproval, "Your registration is awaiting approval.");

        if (profile.ApprovalStatus == ApprovalStatus.Rejected)
            return LoginResult.Failure(LoginFailureReason.Rejected, "Your registration has been rejected.");

        if (profile.AccountStatus != AccountStatus.Active)
            return LoginResult.Failure(LoginFailureReason.AccountInactive, "Your account is not active.");

        var roles = await _identityService.GetRolesAsync(identityUser.Id, cancellationToken);
        var token = _jwtTokenService.CreateToken(new JwtTokenUser(
            identityUser.Id,
            profile.Id,
            identityUser.Email,
            profile.FirstName,
            profile.LastName,
            roles));

        return LoginResult.Success(
            token.AccessToken,
            token.ExpiresAtUtc,
            profile.Id,
            identityUser.Email,
            profile.FirstName,
            profile.LastName,
            roles);
    }
}
