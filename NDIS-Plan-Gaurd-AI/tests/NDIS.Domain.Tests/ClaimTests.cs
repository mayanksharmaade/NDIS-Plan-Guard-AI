using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Tests;

public class ClaimTests
{
    [Fact]
    public void New_claim_is_draft()
    {
        var claim = NewClaim("CLM-001");

        Assert.Equal(ClaimStatus.Draft, claim.Status);
    }

    [Fact]
    public void Submit_changes_claim_status()
    {
        var claim = NewClaim("CLM-002");

        claim.Submit();

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.NotNull(claim.SubmittedAtUtc);
    }

    [Fact]
    public void Review_workflow_can_approve_submitted_claim()
    {
        var claim = NewClaim("CLM-003");
        claim.Submit();

        claim.StartReview();
        claim.ApplyReviewDecision(ClaimReviewOutcome.Approved);

        Assert.Equal(ClaimStatus.Approved, claim.Status);
        Assert.NotNull(claim.ReviewStartedAtUtc);
        Assert.NotNull(claim.DecidedAtUtc);
    }

    [Fact]
    public void More_information_claim_can_be_updated_and_resubmitted()
    {
        var claim = NewClaim("CLM-004");
        claim.Submit();
        claim.StartReview();
        claim.ApplyReviewDecision(ClaimReviewOutcome.MoreInformationRequired);

        claim.UpdateDraft(
            DateTime.Today.AddDays(-1),
            DateTime.Today,
            "Core Supports",
            "Updated information supplied by provider",
            150m);
        claim.Submit();

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Null(claim.ReviewStartedAtUtc);
        Assert.Null(claim.DecidedAtUtc);
    }

    [Fact]
    public void Draft_claim_cannot_receive_review_decision()
    {
        var claim = NewClaim("CLM-005");

        var exception = Assert.Throws<InvalidOperationException>(
            () => claim.ApplyReviewDecision(ClaimReviewOutcome.Rejected));

        Assert.Contains("processing", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Claim NewClaim(string claimNumber) => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        claimNumber,
        DateTime.Today,
        DateTime.Today,
        "Core Supports",
        "Support service",
        100m);
}
