using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews.Models
{
    public sealed record MlDecisionSupportDto
    {
        public bool Available { get; init; }

        public decimal? Probability { get; init; }

        public int? Score { get; init; }

        public string? RiskBand { get; init; }

        public string? ModelVersion { get; init; }

        public string? FeatureVersion { get; init; }

        public DateTimeOffset? ScoredAtUtc { get; init; }

        public string? FailureReason { get; init; }

        public IReadOnlyCollection<MlRiskFactorDto> Factors { get; init; }
            = Array.Empty<MlRiskFactorDto>();
    }

    public sealed record MlRiskFactorDto
    {
        public required string Feature { get; init; }

        public required string Description { get; init; }

        public string? ObservedValue { get; init; }

        public string? BaselineValue { get; init; }

        public double ProbabilityImpact { get; init; }
    }
}
