using NDIS.Domain.Enums;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class ServiceProviderEmployee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceProviderId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public EmployeeRole Role { get; set; } = EmployeeRole.SupportWorker;
    public string? Qualifications { get; set; }
    public decimal? DefaultHourlyRate { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ServiceProvider ServiceProvider { get; set; } = null!;

    public ICollection<ParticipantServiceAssignment> ParticipantAssignments { get; set; }
        = new List<ParticipantServiceAssignment>();

    public ICollection<ServiceDelivery> ServiceDeliveries { get; set; }
        = new List<ServiceDelivery>();
}
