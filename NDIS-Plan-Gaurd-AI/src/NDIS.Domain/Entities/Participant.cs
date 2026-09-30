using NDIS.Domain.Common;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class Participant : AuditableEntity
{
    private Participant() { }

    public Participant(
        Guid serviceProviderId,
        string ndisNumber,
        string firstName,
        string lastName,
        DateTime? dateOfBirth,
        string? email,
        string? phoneNumber,
        DateTime? planStartDate,
        DateTime? planEndDate,
        string EmergencyContactName,
        string EmergyContactRelationship,
        string EmergencyContactPhoneNumber,
        decimal? planTotalBudget = null)
    {
        if (planTotalBudget.HasValue && planTotalBudget.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(planTotalBudget), "Plan total budget must be greater than zero when supplied.");

        ServiceProviderId = serviceProviderId;
        NdisNumber = ndisNumber;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        PhoneNumber = phoneNumber;
        PlanStartDate = planStartDate;
        PlanEndDate = planEndDate;
        PlanTotalBudget = planTotalBudget;
        Status = ParticipantStatus.Active;
    }

    public Guid ServiceProviderId { get; private set; }
    public string NdisNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime? DateOfBirth { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTime? PlanStartDate { get; private set; }
    public DateTime? PlanEndDate { get; private set; }
    public string? EmergencyContactName { get; private set; }

    public string? EmergencyContactRelationship { get; private set; }

    public string? EmergencyContactPhoneNumber { get; private set; }
    public decimal? PlanTotalBudget { get; private set; }
    public ParticipantStatus Status { get; private set; }

    public ServiceProvider ServiceProvider { get; private set; } = null!;

    public void Update(
        string firstName,
        string lastName,
        DateTime? dateOfBirth,
        string? email,
        string? phoneNumber,
        DateTime? planStartDate,
        DateTime? planEndDate,
        string? EmergencyContactName,
        string? EmergyContactRelationship,
        string? EmergencyContactPhoneNumber,
        decimal? planTotalBudget = null)
    {
        if (planTotalBudget.HasValue && planTotalBudget.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(planTotalBudget), "Plan total budget must be greater than zero when supplied.");

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Email = email;
        PhoneNumber = phoneNumber;
        PlanStartDate = planStartDate;
        PlanEndDate = planEndDate;
        
        PlanTotalBudget = planTotalBudget ?? PlanTotalBudget;
        MarkUpdated();
    }

    public void Deactivate()
    {
        Status = ParticipantStatus.Inactive;
        MarkUpdated();
    }

    public void Reactivate()
    {
        Status = ParticipantStatus.Active;
        MarkUpdated();
    }

    public void Archive()
    {
        Status = ParticipantStatus.Archived;
        MarkUpdated();
    }
}
