using NDIS.Application.Abstractions.Risk;

namespace NDIS.Application.Claims;

public sealed record ClaimListItemDto(
    Guid Id,
    string ClaimNumber,
    Guid ParticipantId,
    string ParticipantName,
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    decimal Amount,
    int? Units,
    decimal? UnitPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc);

public sealed record ClaimServiceDeliveryDto(
    Guid Id,
    Guid ServiceProviderEmployeeId,
    string EmployeeName,
    string SupportCategory,
    DateTimeOffset ServiceStartUtc,
    DateTimeOffset ServiceEndUtc,
    decimal ServiceHours,
    decimal HourlyRate,
    decimal Amount,
    string? ServiceLocation);

public sealed record EligibleServiceDeliveryDto(
    Guid Id,
    Guid ParticipantServiceAssignmentId,
    Guid ServiceProviderEmployeeId,
    string EmployeeName,
    string SupportCategory,
    DateTimeOffset ServiceStartUtc,
    DateTimeOffset ServiceEndUtc,
    decimal ServiceHours,
    decimal AgreedHourlyRate,
    decimal Amount,
    string? ServiceLocation);

public sealed record ClaimDetailsDto(
    Guid Id,
    string ClaimNumber,
    Guid ServiceProviderId,
    Guid ParticipantId,
    string ParticipantNdisNumber,
    string ParticipantName,
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    string Description,
    decimal Amount,
    int? Units,
    decimal? UnitPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc,
    ClaimRiskAssessmentDto? RiskAssessment = null,
    decimal? TotalServiceHours = null,
    decimal? AgreedHourlyRate = null,
    string? EmployeeName = null,
    IReadOnlyCollection<ClaimServiceDeliveryDto>? ServiceDeliveries = null);
