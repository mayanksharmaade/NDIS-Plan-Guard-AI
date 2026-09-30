namespace NDIS.Application.Abstractions.Identity;

public sealed record IdentityUserInfo(
    Guid Id,
    string Email,
    string? PhoneNumber);
