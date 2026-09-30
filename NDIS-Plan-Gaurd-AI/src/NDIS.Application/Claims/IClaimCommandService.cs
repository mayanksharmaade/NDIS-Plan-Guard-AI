namespace NDIS.Application.Claims;

public interface IClaimCommandService
{
    Task<ClaimOperationResult> CreateAsync(
        CreateClaimCommand command,
        CancellationToken cancellationToken = default);

    Task<ClaimOperationResult> UpdateAsync(
        Guid claimId,
        UpdateClaimCommand command,
        CancellationToken cancellationToken = default);

    Task<ClaimOperationResult> SubmitAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);
}
