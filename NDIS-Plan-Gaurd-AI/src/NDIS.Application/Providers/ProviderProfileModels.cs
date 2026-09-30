namespace NDIS.Application.Providers;

public sealed record ServiceProviderProfileDto(
    Guid ServiceProviderId,
    Guid UserProfileId,
    string Email,
    string FirstName,
    string LastName,
    string LegalName,
    string? TradingName,
    string Abn,
    string? NdisRegistrationNumber,
    string ApprovalStatus,
    string Status);

public sealed record UpdateServiceProviderProfileCommand(
    string FirstName,
    string LastName,
    string LegalName,
    string? TradingName,
    string Abn,
    string? NdisRegistrationNumber);

public sealed record ProviderProfileOperationResult(bool Succeeded, string? Error)
{
    public static ProviderProfileOperationResult Success() => new(true, null);
    public static ProviderProfileOperationResult Failure(string error) => new(false, error);
}
