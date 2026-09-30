namespace NDIS.Contracts.Auditing;

public sealed record AuditEventResponse(
    Guid Id,
    Guid? ServiceProviderId,
    string EntityType,
    Guid EntityId,
    string Action,
    string Description,
    Guid? ActorIdentityUserId,
    string? ActorEmail,
    DateTime OccurredAtUtc);
