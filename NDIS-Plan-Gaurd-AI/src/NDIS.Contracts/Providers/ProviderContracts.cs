namespace NDIS.Contracts.Providers;

public sealed record ServiceProviderProfileResponse(
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

public sealed record UpdateServiceProviderProfileRequest(
    string FirstName,
    string LastName,
    string LegalName,
    string? TradingName,
    string Abn,
    string? NdisRegistrationNumber);
