using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Infrastructure.RiskScoring;

public sealed record PythonRiskFactor
{
    public required string Feature { get; init; }

    public required string Description { get; init; }

    public object? ObservedValue { get; init; }

    public object? BaselineValue { get; init; }

    public double ProbabilityImpact { get; init; }
}

public sealed record PythonRiskScoreResponse
{
    public required string ClaimId { get; init; }

    public double RiskProbability { get; init; }

    public int RiskScore { get; init; }

    public required string RiskBand { get; init; }

    public IReadOnlyList<PythonRiskFactor> TopFactors { get; init; }
        = Array.Empty<PythonRiskFactor>();

    public required string ModelVersion { get; init; }

    public required string FeatureVersion { get; init; }

    public DateTimeOffset ScoredAtUtc { get; init; }

    public string? DecisionSupportNotice { get; init; }
}
