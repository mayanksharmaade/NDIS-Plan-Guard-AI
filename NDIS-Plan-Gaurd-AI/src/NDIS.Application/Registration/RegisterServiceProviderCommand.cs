namespace NDIS.Application.Registration;

public sealed record RegisterServiceProviderCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string LegalName,
    string? TradingName,
    string Abn,
    string? NdisRegistrationNumber,
    string? PhoneNumber);
