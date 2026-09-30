namespace NDIS.Application.Abstractions.Security;

public sealed record JwtTokenUser(
    Guid IdentityUserId,
    Guid UserProfileId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);

public sealed record JwtTokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(JwtTokenUser user);
}
