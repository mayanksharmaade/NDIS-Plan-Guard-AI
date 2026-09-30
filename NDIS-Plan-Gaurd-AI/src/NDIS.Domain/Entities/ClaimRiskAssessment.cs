using NDIS.Domain.Common;

namespace NDIS.Domain.Entities;

public sealed class ClaimRiskAssessment : Entity
{
    private ClaimRiskAssessment() { }

    public ClaimRiskAssessment(
        Guid claimId,
        bool available,
        decimal? probability,
        int? score,
        string? riskBand,
        string? modelVersion,
        string? featureVersion,
        DateTimeOffset? scoredAtUtc,
        string? failureReason)
    {
        ClaimId = claimId;
        Available = available;
        Probability = probability;
        Score = score;
        RiskBand = Clean(riskBand);
        ModelVersion = Clean(modelVersion);
        FeatureVersion = Clean(featureVersion);
        ScoredAtUtc = scoredAtUtc;
        FailureReason = Clean(failureReason);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid ClaimId { get; private set; }
    public bool Available { get; private set; }
    public decimal? Probability { get; private set; }
    public int? Score { get; private set; }
    public string? RiskBand { get; private set; }
    public string? ModelVersion { get; private set; }
    public string? FeatureVersion { get; private set; }
    public DateTimeOffset? ScoredAtUtc { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Claim Claim { get; private set; } = null!;
    public ICollection<ClaimRiskFactor> Factors { get; private set; } = new List<ClaimRiskFactor>();

    public void AddFactor(
        string feature,
        string description,
        string? observedValueJson,
        string? baselineValueJson,
        double probabilityImpact)
    {
        Factors.Add(new ClaimRiskFactor(
            Id,
            feature,
            description,
            observedValueJson,
            baselineValueJson,
            probabilityImpact));
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
