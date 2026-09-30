namespace NDIS.Application.Abstractions.Risk;

public sealed record ClaimRiskFactorDto(
    string Feature,
    string Description,
    object? ObservedValue,
    object? BaselineValue,
    double ProbabilityImpact);

public sealed record ClaimRiskAssessmentDto(
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
    IReadOnlyCollection<ClaimRiskFactorDto> Factors);
