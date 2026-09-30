namespace NDIS.Domain.Entities;

public sealed class ParticipantServiceAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParticipantId { get; set; }
    public Guid ServiceProviderEmployeeId { get; set; }

    public string SupportCategory { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? AgreedHourlyRate { get; set; }
    public bool IsActive { get; set; } = true;

    public Participant Participant { get; set; } = null!;
    public ServiceProviderEmployee ServiceProviderEmployee { get; set; } = null!;
}
