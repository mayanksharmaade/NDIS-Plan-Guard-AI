using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Abstractions.Risk
{

    public sealed record RiskAssessmentResult
    {
        public bool Available { get; init; }

        public decimal? Probability { get; init; }

        public int? Score { get; init; }

        public string? RiskBand { get; init; }

        public IReadOnlyCollection<RiskFactorResult> Factors { get; init; }
            = Array.Empty<RiskFactorResult>();

        public string? ModelVersion { get; init; }

        public string? FeatureVersion { get; init; }

        public DateTimeOffset? ScoredAtUtc { get; init; }

        public string? FailureReason { get; init; }

        public static RiskAssessmentResult Unavailable(
            string failureReason)
        {
            return new RiskAssessmentResult
            {
                Available = false,
                FailureReason = failureReason
            };
        }
    }
}
