using NDIS.Domain.Common;
using NDIS.Domain.Enums;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public class ServiceProvider : AuditableEntity
{
    private ServiceProvider()
    {
    }

    public ServiceProvider(
        string legalName,
        string? tradingName,
        string abn,
        string? ndisRegistrationNumber)
    {
        LegalName = legalName;
        TradingName = tradingName;
        Abn = abn;
        NdisRegistrationNumber = ndisRegistrationNumber;

        ApprovalStatus = ApprovalStatus.Pending;
        Status = ServiceProviderStatus.Inactive;
    }

    public string LegalName { get; private set; } = string.Empty;

    public string? TradingName { get; private set; }

    public string Abn { get; private set; } = string.Empty;

    public string? NdisRegistrationNumber { get; private set; }

    public ApprovalStatus ApprovalStatus { get; private set; }

    public ServiceProviderStatus Status { get; private set; }

    public DateTime? ApprovedAtUtc { get; private set; }

    public DateTime? RejectedAtUtc { get; private set; }

    public ICollection<ServiceProviderUser> Users { get; private set; }
        = new List<ServiceProviderUser>();

    public void Approve()
    {
        ApprovalStatus = ApprovalStatus.Approved;
        Status = ServiceProviderStatus.Active;

        ApprovedAtUtc = DateTime.UtcNow;
        RejectedAtUtc = null;

        MarkUpdated();
    }

    public void Reject()
    {
        ApprovalStatus = ApprovalStatus.Rejected;
        Status = ServiceProviderStatus.Inactive;

        RejectedAtUtc = DateTime.UtcNow;
        ApprovedAtUtc = null;

        MarkUpdated();
    }

    public void Suspend()
    {
        Status = ServiceProviderStatus.Suspended;
        MarkUpdated();
    }

    public void Reactivate()
    {
        if (ApprovalStatus != ApprovalStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved service providers can be activated.");
        }

        Status = ServiceProviderStatus.Active;
        MarkUpdated();
    }

    public void Close()
    {
        Status = ServiceProviderStatus.Closed;
        MarkUpdated();
    }

    public void UpdateDetails(
        string legalName,
        string? tradingName,
        string abn,
        string? ndisRegistrationNumber)
    {
        LegalName = legalName;
        TradingName = tradingName;
        Abn = abn;
        NdisRegistrationNumber = ndisRegistrationNumber;

        MarkUpdated();
    }
}