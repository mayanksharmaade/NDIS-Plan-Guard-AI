using NDIS.Domain.Common;

namespace NDIS.Domain.Entities;

public sealed class ClaimRiskFactor : Entity
{
    private ClaimRiskFactor() { }

    public ClaimRiskFactor(
        Guid claimRiskAssessmentId,
        string feature,
        string description,
        string? observedValueJson,
        string? baselineValueJson,
        double probabilityImpact)
    {
        ClaimRiskAssessmentId = claimRiskAssessmentId;
        Feature = feature.Trim();
        Description = description.Trim();
        ObservedValueJson = observedValueJson;
        BaselineValueJson = baselineValueJson;
        ProbabilityImpact = probabilityImpact;
    }

    public Guid ClaimRiskAssessmentId { get; private set; }
    public string Feature { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ObservedValueJson { get; private set; }
    public string? BaselineValueJson { get; private set; }
    public double ProbabilityImpact { get; private set; }

    public ClaimRiskAssessment ClaimRiskAssessment { get; private set; } = null!;
}
