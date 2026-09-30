namespace NDIS.Application.Reviews;

public interface IClaimReviewService
{
    Task<IReadOnlyCollection<ReviewQueueItemDto>> GetQueueAsync(
        CancellationToken cancellationToken = default);

    Task<ClaimForReviewDto?> GetClaimAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<ClaimReviewOperationResult> StartReviewAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<ClaimReviewOperationResult> RecordDecisionAsync(
        Guid claimId,
        ReviewDecisionCommand command,
        CancellationToken cancellationToken = default);
}
