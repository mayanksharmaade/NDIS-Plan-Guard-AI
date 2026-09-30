namespace NDIS.PlanGuard.RiskScoring.Reference;

public sealed record PythonRiskScoreRequest(
    string ClaimId,
    string? CorrelationId,
    decimal ClaimAmount,
    int Units,
    decimal UnitPrice,
    string ServiceCategory,
    int DaysServiceToSubmission,
    decimal PlanTotalBudget,
    decimal PlanRemainingBefore,
    decimal PlanUtilisationBefore,
    decimal ClaimToRemainingRatio,
    int ParticipantClaims7d,
    int ParticipantClaims30d,
    int ParticipantClaims90d,
    decimal? ParticipantAvgClaimPrior,
    decimal ClaimToParticipantAvg,
    int ProviderClaims7d,
    int ProviderClaims30d,
    int ProviderClaims90d,
    decimal? ProviderAvgClaimPrior,
    decimal ClaimToProviderAvg,
    int? DaysSinceParticipantPreviousClaim,
    int ValidationFindingCount,
    int ValidationHighCount,
    int DuplicateWarning,
    int PlanLimitWarning);

public sealed record PythonRiskFactor(
    string Feature,
    string Description,
    object? ObservedValue,
    object? BaselineValue,
    double ProbabilityImpact);

public sealed record PythonRiskScoreResponse(
    string ClaimId,
    double RiskProbability,
    int RiskScore,
    string RiskBand,
    IReadOnlyList<PythonRiskFactor> TopFactors,
    string ModelVersion,
    string FeatureVersion,
    DateTimeOffset ScoredAtUtc,
    string DecisionSupportNotice);
