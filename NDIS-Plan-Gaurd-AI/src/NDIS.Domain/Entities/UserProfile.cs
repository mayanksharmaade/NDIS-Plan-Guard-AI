using NDIS.Domain.Common;
using NDIS.Domain.Enums;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public class UserProfile : AuditableEntity
{
    private UserProfile()
    {
    }

    public UserProfile(
        Guid identityUserId,
        string firstName,
        string lastName,
        UserCategory userCategory)
    {
        IdentityUserId = identityUserId;
        FirstName = firstName;
        LastName = lastName;
        UserCategory = userCategory;

        ApprovalStatus = ApprovalStatus.Pending;
        AccountStatus = AccountStatus.Inactive;
    }

    public Guid IdentityUserId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public UserCategory UserCategory { get; private set; }

    public ApprovalStatus ApprovalStatus { get; private set; }

    public AccountStatus AccountStatus { get; private set; }

    public DateTime? ApprovedAtUtc { get; private set; }

    public DateTime? RejectedAtUtc { get; private set; }

    public ICollection<ServiceProviderUser> ServiceProviderMemberships { get; private set; }
        = new List<ServiceProviderUser>();

    public void Approve()
    {
        ApprovalStatus = ApprovalStatus.Approved;
        AccountStatus = AccountStatus.Active;
        ApprovedAtUtc = DateTime.UtcNow;
        RejectedAtUtc = null;

        MarkUpdated();
    }

    public void Reject()
    {
        ApprovalStatus = ApprovalStatus.Rejected;
        AccountStatus = AccountStatus.Inactive;
        RejectedAtUtc = DateTime.UtcNow;
        ApprovedAtUtc = null;

        MarkUpdated();
    }

    public void Suspend()
    {
        AccountStatus = AccountStatus.Suspended;
        MarkUpdated();
    }

    public void Disable()
    {
        AccountStatus = AccountStatus.Disabled;
        MarkUpdated();
    }

    public void Activate()
    {
        if (ApprovalStatus != ApprovalStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved users can be activated.");
        }

        AccountStatus = AccountStatus.Active;
        MarkUpdated();
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;

        MarkUpdated();
    }
}