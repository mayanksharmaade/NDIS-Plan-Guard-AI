namespace NDIS.Contracts.Risk;

public sealed record ClaimRiskFactorResponse(
    string Feature,
    string Description,
    object? ObservedValue,
    object? BaselineValue,
    double ProbabilityImpact);

public sealed record ClaimRiskAssessmentResponse(
    Guid Id,
    Guid ClaimId,
    bool Available,
    decimal? Probability,
    int? Score,
    string? RiskBand,
    string? ModelVersion,
    string? FeatureVersion,
    DateTimeOffset? ScoredAtUtc,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<ClaimRiskFactorResponse> Factors);
