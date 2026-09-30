namespace NDIS.Application.Auditing;

public interface IAuditTrailService
{
    void Add(AddAuditEventCommand command);

    Task<IReadOnlyCollection<AuditEventDto>> GetEventsAsync(
        int take = 100,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default);
}
