namespace NDIS.Application.Administration;

public sealed record CreateAdminCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber);

public sealed record CreateAdminResult(
    bool Succeeded,
    Guid? UserProfileId,
    IReadOnlyCollection<string> Errors)
{
    public static CreateAdminResult Success(Guid userProfileId) =>
        new(true, userProfileId, Array.Empty<string>());

    public static CreateAdminResult Failure(params string[] errors) =>
        new(false, null, errors);
}

public sealed record PendingRegistrationDto(
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

public sealed record UserSummaryDto(
    Guid UserProfileId,
    Guid IdentityUserId,
    string Email,
    string FirstName,
    string LastName,
    string UserCategory,
    string ApprovalStatus,
    string AccountStatus,
    IReadOnlyCollection<string> Roles);

public sealed record AdminOperationResult(bool Succeeded, string? Error)
{
    public static AdminOperationResult Success() => new(true, null);
    public static AdminOperationResult Failure(string error) => new(false, error);
}
