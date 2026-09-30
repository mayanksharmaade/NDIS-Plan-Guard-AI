using System;
using System.Collections.Generic;
using System.Text;

using NDIS.Application.Abstractions.Risk;

namespace NDIS.Infrastructure.RiskScoring;

public sealed class PythonRiskScoringService
    : IRiskScoringService
{
    private readonly PythonRiskScoringClient _client;

    public PythonRiskScoringService(
        PythonRiskScoringClient client)
    {
        _client = client;
    }

    public async Task<RiskAssessmentResult> ScoreAsync(
        ClaimRiskContext context,
        CancellationToken cancellationToken = default)
    {
        var request = new PythonRiskScoreRequest
        {
            ClaimId = context.ClaimId,

            CorrelationId = context.CorrelationId,

            ClaimAmount = context.ClaimAmount,

            Units = context.Units,

            UnitPrice = context.UnitPrice,

            ServiceCategory =
                context.ServiceCategory,

            DaysServiceToSubmission =
                context.DaysServiceToSubmission,

            PlanTotalBudget =
                context.PlanTotalBudget,

            PlanRemainingBefore =
                context.PlanRemainingBefore,

            PlanUtilisationBefore =
                context.PlanUtilisationBefore,

            ClaimToRemainingRatio =
                context.ClaimToRemainingRatio,

            ParticipantClaims7d =
                context.ParticipantClaims7d,

            ParticipantClaims30d =
                context.ParticipantClaims30d,

            ParticipantClaims90d =
                context.ParticipantClaims90d,

            ParticipantAvgClaimPrior =
                context.ParticipantAvgClaimPrior,

            ClaimToParticipantAvg =
                context.ClaimToParticipantAvg,

            ProviderClaims7d =
                context.ProviderClaims7d,

            ProviderClaims30d =
                context.ProviderClaims30d,

            ProviderClaims90d =
                context.ProviderClaims90d,

            ProviderAvgClaimPrior =
                context.ProviderAvgClaimPrior,

            ClaimToProviderAvg =
                context.ClaimToProviderAvg,

            DaysSinceParticipantPreviousClaim =
                context.DaysSinceParticipantPreviousClaim,

            ValidationFindingCount =
                context.ValidationFindingCount,

            ValidationHighCount =
                context.ValidationHighCount,

            DuplicateWarning =
                context.DuplicateWarning ? 1 : 0,

            PlanLimitWarning =
                context.PlanLimitWarning ? 1 : 0
        };

        var response =
            await _client.ScoreAsync(
                request,
                cancellationToken);

        if (response is null)
        {
            return RiskAssessmentResult.Unavailable(
                "ML risk service is currently unavailable.");
        }

        var factors =
            response.TopFactors
                .Select(
                    factor =>
                        new RiskFactorResult
                        {
                            Feature =
                                factor.Feature,

                            Description =
                                factor.Description,

                            ObservedValue =
                                factor.ObservedValue,

                            BaselineValue =
                                factor.BaselineValue,

                            ProbabilityImpact =
                                factor.ProbabilityImpact
                        })
                .ToArray();

        return new RiskAssessmentResult
        {
            Available = true,

            Probability =
                (decimal)response.RiskProbability,

            Score =
                response.RiskScore,

            RiskBand =
                response.RiskBand,

            Factors =
                factors,

            ModelVersion =
                response.ModelVersion,

            FeatureVersion =
                response.FeatureVersion,

            ScoredAtUtc =
                response.ScoredAtUtc,

            FailureReason =
                null
        };
    }
}
