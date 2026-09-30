using NDIS.Contracts.Risk;

namespace NDIS.Contracts.Claims;

public sealed record ClaimListItemResponse(
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

public sealed record ClaimServiceDeliveryResponse(
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

public sealed record EligibleServiceDeliveryResponse(
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

public sealed record ClaimDetailsResponse(
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
    ClaimRiskAssessmentResponse? RiskAssessment = null,
    decimal? TotalServiceHours = null,
    decimal? AgreedHourlyRate = null,
    string? EmployeeName = null,
    IReadOnlyCollection<ClaimServiceDeliveryResponse>? ServiceDeliveries = null);

public sealed record CreateClaimRequest(
    Guid ParticipantId,
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    string Description,
    decimal Amount,
    int? Units = null,
    decimal? UnitPrice = null,
    IReadOnlyCollection<Guid>? ServiceDeliveryIds = null);

public sealed record UpdateClaimRequest(
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    string Description,
    decimal Amount,
    int? Units = null,
    decimal? UnitPrice = null);

public sealed record ClaimMutationResponse(
    Guid ClaimId,
    string ClaimNumber,
    string Message);
