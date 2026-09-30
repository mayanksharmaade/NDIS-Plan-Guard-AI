namespace NDIS.Contracts.Identity;
public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserProfileId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);
