namespace NDIS.Application.Abstractions.Risk;

public interface IClaimRiskFeatureBuilder
{
    Task<ClaimRiskFeatureBuildResult> BuildAsync(
        Guid claimId,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}

public sealed record ClaimRiskFeatureBuildResult(
    bool Succeeded,
    ClaimRiskContext? Context,
    string? Error)
{
    public static ClaimRiskFeatureBuildResult Success(ClaimRiskContext context) =>
        new(true, context, null);

    public static ClaimRiskFeatureBuildResult Failure(string error) =>
        new(false, null, error);
}
