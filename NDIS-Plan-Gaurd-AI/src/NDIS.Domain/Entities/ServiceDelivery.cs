namespace NDIS.Domain.Entities;

public sealed class ServiceDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParticipantId { get; set; }
    public Guid ServiceProviderEmployeeId { get; set; }
    public Guid? ParticipantServiceAssignmentId { get; set; }

    // Stores the stable support-service code from SupportServiceCatalog.
    public string SupportCategory { get; set; } = string.Empty;
    public DateTimeOffset ServiceStartUtc { get; set; }
    public DateTimeOffset ServiceEndUtc { get; set; }

    public decimal ServiceHours { get; set; }

    // Snapshot of the participant/provider agreed billing rate at delivery time.
    // This is intentionally separate from the employee's internal pay rate.
    public decimal HourlyRate { get; set; }
    public decimal Amount { get; set; }

    public string? ServiceLocation { get; set; }
    public string? Notes { get; set; }
    public bool IsCancelled { get; set; }

    public Participant Participant { get; set; } = null!;
    public ServiceProviderEmployee ServiceProviderEmployee { get; set; } = null!;
    public ParticipantServiceAssignment? ParticipantServiceAssignment { get; set; }

    public ICollection<ClaimServiceDelivery> ClaimLinks { get; set; }
        = new List<ClaimServiceDelivery>();
}
