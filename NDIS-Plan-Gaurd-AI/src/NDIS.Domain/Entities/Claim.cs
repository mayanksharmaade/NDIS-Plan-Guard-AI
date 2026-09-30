using NDIS.Domain.Common;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class Claim : AuditableEntity
{
    private Claim() { }

    public Claim(
        Guid serviceProviderId,
        Guid participantId,
        string claimNumber,
        DateTime serviceFrom,
        DateTime serviceTo,
        string supportCategory,
        string description,
        decimal amount,
        int? units = null,
        decimal? unitPrice = null,
        decimal? totalServiceHours = null,
        decimal? agreedHourlyRate = null)
    {
        Validate(serviceFrom, serviceTo, supportCategory, description, amount, units, unitPrice, totalServiceHours, agreedHourlyRate);

        ServiceProviderId = serviceProviderId;
        ParticipantId = participantId;
        ClaimNumber = claimNumber;
        ServiceFrom = serviceFrom.Date;
        ServiceTo = serviceTo.Date;
        SupportCategory = supportCategory.Trim();
        Description = description.Trim();
        Amount = amount;
        Units = units;
        TotalServiceHours = totalServiceHours;
        AgreedHourlyRate = agreedHourlyRate;
        UnitPrice = unitPrice
            ?? agreedHourlyRate
            ?? (units.HasValue && units.Value > 0 ? amount / units.Value : null);
        Status = ClaimStatus.Draft;
    }

    public Guid ServiceProviderId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public string ClaimNumber { get; private set; } = string.Empty;
    public DateTime ServiceFrom { get; private set; }
    public DateTime ServiceTo { get; private set; }
    public string SupportCategory { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public int? Units { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? TotalServiceHours { get; private set; }
    public decimal? AgreedHourlyRate { get; private set; }
    public ClaimStatus Status { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? ReviewStartedAtUtc { get; private set; }
    public DateTime? DecidedAtUtc { get; private set; }
    public Guid? ServiceDeliveryId { get; set; }

    public Guid? ServiceProviderEmployeeId { get; set; }

    public ServiceDelivery? ServiceDelivery { get; set; }

    public ServiceProviderEmployee? ServiceProviderEmployee { get; set; }

    public ICollection<ClaimServiceDelivery> ServiceDeliveries { get; private set; }
        = new List<ClaimServiceDelivery>();

    public ServiceProvider ServiceProvider { get; private set; } = null!;
    public Participant Participant { get; private set; } = null!;

    public void UpdateDraft(
        DateTime serviceFrom,
        DateTime serviceTo,
        string supportCategory,
        string description,
        decimal amount,
        int? units = null,
        decimal? unitPrice = null)
    {
        if (Status is not ClaimStatus.Draft and not ClaimStatus.MoreInformationRequired)
            throw new InvalidOperationException("Only draft claims or claims awaiting more information can be edited.");

        Validate(serviceFrom, serviceTo, supportCategory, description, amount, units, unitPrice, null, null);

        ServiceFrom = serviceFrom.Date;
        ServiceTo = serviceTo.Date;
        SupportCategory = supportCategory.Trim();
        Description = description.Trim();
        Amount = amount;
        Units = units ?? Units;
        UnitPrice = unitPrice ?? (Units.HasValue && Units.Value > 0 ? amount / Units.Value : UnitPrice);
        MarkUpdated();
    }

    public void Submit()
    {
        if (Status is not ClaimStatus.Draft and not ClaimStatus.MoreInformationRequired)
            throw new InvalidOperationException("Only draft claims or claims awaiting more information can be submitted.");

        Status = ClaimStatus.Submitted;
        SubmittedAtUtc = DateTime.UtcNow;
        ReviewStartedAtUtc = null;
        DecidedAtUtc = null;
        MarkUpdated();
    }

    public void StartReview()
    {
        if (Status is not ClaimStatus.Submitted and not ClaimStatus.ReviewRequired)
            throw new InvalidOperationException("Only submitted claims or claims requiring review can enter review.");

        Status = ClaimStatus.Processing;
        ReviewStartedAtUtc = DateTime.UtcNow;
        MarkUpdated();
    }

    public void ApplyReviewDecision(ClaimReviewOutcome outcome)
    {
        if (Status != ClaimStatus.Processing)
            throw new InvalidOperationException("A claim must be in processing before a review decision can be recorded.");

        Status = outcome switch
        {
            ClaimReviewOutcome.Approved => ClaimStatus.Approved,
            ClaimReviewOutcome.Rejected => ClaimStatus.Rejected,
            ClaimReviewOutcome.MoreInformationRequired => ClaimStatus.MoreInformationRequired,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };

        DecidedAtUtc = DateTime.UtcNow;
        MarkUpdated();
    }

    private static void Validate(
        DateTime serviceFrom,
        DateTime serviceTo,
        string supportCategory,
        string description,
        decimal amount,
        int? units,
        decimal? unitPrice,
        decimal? totalServiceHours,
        decimal? agreedHourlyRate)
    {
        if (serviceTo.Date < serviceFrom.Date)
            throw new ArgumentException("Service-to date cannot be before service-from date.");

        if (string.IsNullOrWhiteSpace(supportCategory))
            throw new ArgumentException("Support category is required.", nameof(supportCategory));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Claim amount must be greater than zero.");

        if (units.HasValue && units.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(units), "Claim units must be greater than zero when supplied.");

        if (unitPrice.HasValue && !units.HasValue && !totalServiceHours.HasValue)
            throw new ArgumentException("Claim units or service hours are required when unit price is supplied.", nameof(units));

        if (totalServiceHours.HasValue && totalServiceHours.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalServiceHours), "Service hours must be greater than zero when supplied.");

        if (agreedHourlyRate.HasValue && agreedHourlyRate.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(agreedHourlyRate), "Agreed hourly rate must be greater than zero when supplied.");

        if (unitPrice.HasValue && unitPrice.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Claim unit price must be greater than zero when supplied.");
    }
}
