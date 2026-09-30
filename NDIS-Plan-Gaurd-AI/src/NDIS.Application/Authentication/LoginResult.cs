namespace NDIS.Application.Authentication;

public sealed record LoginResult(
    bool Succeeded,
    string? AccessToken,
    DateTime? ExpiresAtUtc,
    Guid? UserProfileId,
    string? Email,
    string? FirstName,
    string? LastName,
    IReadOnlyCollection<string> Roles,
    LoginFailureReason FailureReason,
    string? Error)
{
    public static LoginResult Success(
        string accessToken,
        DateTime expiresAtUtc,
        Guid userProfileId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> roles) =>
        new(true, accessToken, expiresAtUtc, userProfileId, email, firstName, lastName, roles,
            LoginFailureReason.None, null);

    public static LoginResult Failure(LoginFailureReason reason, string error) =>
        new(false, null, null, null, null, null, null, Array.Empty<string>(), reason, error);
}
