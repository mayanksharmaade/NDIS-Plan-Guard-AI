namespace NDIS.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    {
        CreatedAtUtc = DateTime.UtcNow;
    }

    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    public void MarkUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
