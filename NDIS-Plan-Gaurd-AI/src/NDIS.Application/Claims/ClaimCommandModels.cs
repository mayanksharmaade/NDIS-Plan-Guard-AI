namespace NDIS.Application.Claims;

public sealed record CreateClaimCommand(
    Guid ParticipantId,
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    string Description,
    decimal Amount,
    int? Units = null,
    decimal? UnitPrice = null,
    IReadOnlyCollection<Guid>? ServiceDeliveryIds = null);

public sealed record UpdateClaimCommand(
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    string Description,
    decimal Amount,
    int? Units = null,
    decimal? UnitPrice = null);

public sealed record ClaimOperationResult(
    bool Succeeded,
    Guid? ClaimId,
    string? ClaimNumber,
    string? Error)
{
    public static ClaimOperationResult Success(Guid claimId, string claimNumber) =>
        new(true, claimId, claimNumber, null);

    public static ClaimOperationResult Failure(string error) =>
        new(false, null, null, error);
}
