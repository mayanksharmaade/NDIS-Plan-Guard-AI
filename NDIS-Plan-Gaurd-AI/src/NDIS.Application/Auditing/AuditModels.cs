using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Auditing;

public sealed record AuditEventDto(
    Guid Id,
    Guid? ServiceProviderId,
    string EntityType,
    Guid EntityId,
    string Action,
    string Description,
    Guid? ActorIdentityUserId,
    string? ActorEmail,
    DateTime OccurredAtUtc);

public sealed record AddAuditEventCommand(
    Guid? ServiceProviderId,
    string EntityType,
    Guid EntityId,
    AuditActionType Action,
    string Description);
