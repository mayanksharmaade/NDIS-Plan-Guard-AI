namespace NDIS.Application.Participants;

public sealed record CreateParticipantCommand(
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

public sealed record UpdateParticipantCommand(
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

public sealed record ParticipantDto(
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

public sealed record ParticipantOperationResult(
    bool Succeeded,
    Guid? ParticipantId,
    string? Error)
{
    public static ParticipantOperationResult Success(Guid participantId) => new(true, participantId, null);
    public static ParticipantOperationResult Failure(string error) => new(false, null, error);
}
