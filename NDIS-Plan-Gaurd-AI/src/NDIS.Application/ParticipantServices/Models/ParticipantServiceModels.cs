namespace NDIS.Application.ParticipantServices.Models;

public sealed record ParticipantServiceAssignmentDto(
    Guid Id,
    Guid ParticipantId,
    Guid ServiceProviderEmployeeId,
    string EmployeeName,
    string ProviderName,
    string SupportCategory,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal? EmployeePayRate,
    decimal? AgreedHourlyRate,
    bool IsActive);

public sealed record CreateParticipantServiceAssignmentRequest(
    Guid ServiceProviderEmployeeId,
    string SupportCategory,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal? AgreedHourlyRate);

public sealed record ServiceDeliveryDto(
    Guid Id,
    Guid ParticipantId,
    Guid ServiceProviderEmployeeId,
    Guid? ParticipantServiceAssignmentId,
    string EmployeeName,
    string ProviderName,
    string SupportCategory,
    DateTimeOffset ServiceStartUtc,
    DateTimeOffset ServiceEndUtc,
    decimal ServiceHours,
    decimal HourlyRate,
    decimal Amount,
    string? ServiceLocation,
    string? Notes,
    bool IsClaimed);

public sealed record CreateServiceDeliveryRequest(
    Guid ParticipantServiceAssignmentId,
    DateTimeOffset ServiceStartUtc,
    DateTimeOffset ServiceEndUtc,
    string? ServiceLocation,
    string? Notes);
