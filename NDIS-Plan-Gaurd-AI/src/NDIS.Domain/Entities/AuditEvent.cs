using NDIS.Domain.Common;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class AuditEvent : Entity
{
    private AuditEvent() { }

    public AuditEvent(
        Guid? serviceProviderId,
        string entityType,
        Guid entityId,
        AuditActionType action,
        string description,
        Guid? actorIdentityUserId,
        string? actorEmail)
    {
        ServiceProviderId = serviceProviderId;
        EntityType = entityType.Trim();
        EntityId = entityId;
        Action = action;
        Description = description.Trim();
        ActorIdentityUserId = actorIdentityUserId;
        ActorEmail = string.IsNullOrWhiteSpace(actorEmail) ? null : actorEmail.Trim();
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid? ServiceProviderId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public AuditActionType Action { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? ActorIdentityUserId { get; private set; }
    public string? ActorEmail { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
}
