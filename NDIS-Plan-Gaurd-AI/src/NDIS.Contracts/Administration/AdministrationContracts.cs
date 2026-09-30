namespace NDIS.Contracts.Administration;

public sealed record CreateAdminRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber);

public sealed record CreateAdminResponse(Guid UserProfileId, string Message);

public sealed record PendingRegistrationResponse(
    Guid UserProfileId,
    Guid ServiceProviderId,
    string Email,
    string FirstName,
    string LastName,
    string LegalName,
    string? TradingName,
    string Abn,
    string? NdisRegistrationNumber,
    DateTime RegisteredAtUtc);

public sealed record UserSummaryResponse(
    Guid UserProfileId,
    Guid IdentityUserId,
    string Email,
    string FirstName,
    string LastName,
    string UserCategory,
    string ApprovalStatus,
    string AccountStatus,
    IReadOnlyCollection<string> Roles);
