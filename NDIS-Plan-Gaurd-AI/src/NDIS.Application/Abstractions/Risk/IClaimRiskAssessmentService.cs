namespace NDIS.Application.Abstractions.Risk;

public interface IClaimRiskAssessmentService
{
    Task<ClaimRiskAssessmentDto> ScoreAndPersistAsync(
        Guid claimId,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    Task<ClaimRiskAssessmentDto?> GetLatestAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);
}
