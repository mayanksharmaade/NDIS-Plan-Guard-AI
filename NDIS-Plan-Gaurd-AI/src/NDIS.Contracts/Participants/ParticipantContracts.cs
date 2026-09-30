namespace NDIS.Contracts.Participants;

public sealed record CreateParticipantRequest(
    string NdisNumber,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Email,
    string? PhoneNumber,
    DateTime? PlanStartDate,
    DateTime? PlanEndDate,
    string? EmergencyContactName,
    string? EmergencyContactRelationship,
    string? EmergencyContactPhoneNumber,
    decimal? PlanTotalBudget = null);

public sealed record UpdateParticipantRequest(
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Email,
    string? PhoneNumber,
    DateTime? PlanStartDate,
    DateTime? PlanEndDate,
    string? EmergencyContactName,
    string? EmergencyContactRelationship,
    string? EmergencyContactPhoneNumber,
    decimal? PlanTotalBudget = null);

public sealed record ParticipantResponse(
    Guid Id,
    string NdisNumber,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Email,
    string? PhoneNumber,
    DateTime? PlanStartDate,
    DateTime? PlanEndDate,
    string? EmergencyContactName,
    string? EmergencyContactRelationship,
    string? EmergencyContactPhoneNumber,
    decimal? PlanTotalBudget,
    string Status,
    DateTime CreatedAtUtc);
