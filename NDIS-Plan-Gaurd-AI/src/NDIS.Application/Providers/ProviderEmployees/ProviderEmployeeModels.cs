namespace NDIS.Application.ProviderEmployees.Models;

public sealed record ServiceProviderEmployeeDto(
    Guid Id,
    Guid ServiceProviderId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Role,
    string? Qualifications,
    decimal? DefaultHourlyRate,
    bool IsActive);

public sealed record SaveServiceProviderEmployeeRequest(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Role,
    string? Qualifications,
    decimal? DefaultHourlyRate);
