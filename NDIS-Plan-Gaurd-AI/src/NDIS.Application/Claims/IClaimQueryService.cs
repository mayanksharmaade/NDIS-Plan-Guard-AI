namespace NDIS.Application.Claims;

public interface IClaimQueryService
{
    Task<IReadOnlyCollection<ClaimListItemDto>> GetClaimsAsync(
        CancellationToken cancellationToken = default);

    Task<ClaimDetailsDto?> GetByIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EligibleServiceDeliveryDto>> GetEligibleServiceDeliveriesAsync(
        Guid participantId,
        DateTime serviceFrom,
        DateTime serviceTo,
        CancellationToken cancellationToken = default);
}
