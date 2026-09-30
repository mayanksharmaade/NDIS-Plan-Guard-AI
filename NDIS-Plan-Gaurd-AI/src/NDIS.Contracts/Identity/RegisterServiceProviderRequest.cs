namespace NDIS.Contracts.Identity;

public sealed record RegisterServiceProviderRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string LegalName,
    string? TradingName,
    string Abn,
    string? NdisRegistrationNumber,
    string? PhoneNumber);
