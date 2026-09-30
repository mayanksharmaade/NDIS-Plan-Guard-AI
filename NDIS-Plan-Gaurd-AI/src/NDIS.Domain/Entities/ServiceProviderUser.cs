using NDIS.Domain.Common;
using NDIS.Domain.Enums;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public class ServiceProviderUser : AuditableEntity
{
    private ServiceProviderUser()
    {
    }

    public ServiceProviderUser(
        Guid serviceProviderId,
        Guid userProfileId,
        bool isPrimaryContact = false)
    {
        ServiceProviderId = serviceProviderId;
        UserProfileId = userProfileId;
        IsPrimaryContact = isPrimaryContact;

        Status = ProviderMembershipStatus.Pending;
    }

    public Guid ServiceProviderId { get; private set; }

    public Guid UserProfileId { get; private set; }

    public bool IsPrimaryContact { get; private set; }

    public ProviderMembershipStatus Status { get; private set; }

    public DateTime? ActivatedAtUtc { get; private set; }

    public ServiceProvider ServiceProvider { get; private set; } = null!;

    public UserProfile UserProfile { get; private set; } = null!;

    public void Activate()
    {
        Status = ProviderMembershipStatus.Active;
        ActivatedAtUtc = DateTime.UtcNow;

        MarkUpdated();
    }

    public void Suspend()
    {
        Status = ProviderMembershipStatus.Suspended;
        MarkUpdated();
    }

    public void Remove()
    {
        Status = ProviderMembershipStatus.Removed;
        MarkUpdated();
    }

    public void SetPrimaryContact(bool isPrimaryContact)
    {
        IsPrimaryContact = isPrimaryContact;
        MarkUpdated();
    }
}