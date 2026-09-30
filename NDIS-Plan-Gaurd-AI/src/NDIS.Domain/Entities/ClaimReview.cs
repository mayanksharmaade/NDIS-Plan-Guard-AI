using NDIS.Domain.Common;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class ClaimReview : Entity
{
    private ClaimReview() { }

    public ClaimReview(
        Guid claimId,
        Guid reviewerIdentityUserId,
        string reviewerEmail,
        ClaimReviewOutcome outcome,
        string? comments)
    {
        ClaimId = claimId;
        ReviewerIdentityUserId = reviewerIdentityUserId;
        ReviewerEmail = reviewerEmail.Trim();
        Outcome = outcome;
        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
        ReviewedAtUtc = DateTime.UtcNow;
    }

    public Guid ClaimId { get; private set; }
    public Guid ReviewerIdentityUserId { get; private set; }
    public string ReviewerEmail { get; private set; } = string.Empty;
    public ClaimReviewOutcome Outcome { get; private set; }
    public string? Comments { get; private set; }
    public DateTime ReviewedAtUtc { get; private set; }

    public Claim Claim { get; private set; } = null!;
}
