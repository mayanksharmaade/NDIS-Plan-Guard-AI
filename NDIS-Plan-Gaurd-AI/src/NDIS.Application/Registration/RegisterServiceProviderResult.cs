namespace NDIS.Application.Registration;

public sealed record RegisterServiceProviderResult(
    bool Succeeded,
    Guid? UserProfileId,
    Guid? ServiceProviderId,
    RegistrationFailureReason FailureReason,
    IReadOnlyCollection<string> Errors)
{
    public static RegisterServiceProviderResult Success(
        Guid userProfileId,
        Guid serviceProviderId)
        => new(
            true,
            userProfileId,
            serviceProviderId,
            RegistrationFailureReason.None,
            Array.Empty<string>());

    public static RegisterServiceProviderResult Failure(
        RegistrationFailureReason reason,
        params string[] errors)
        => new(
            false,
            null,
            null,
            reason,
            errors);
}
