using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Abstractions.Risk;

namespace NDIS.Api.Controllers
{
    [ApiController]
    [Route("api/test/risk-scoring")]
    public sealed class RiskScoringTestController : ControllerBase
    {
        private readonly IRiskScoringService _riskScoringService;

        public RiskScoringTestController(
            IRiskScoringService riskScoringService)
        {
            _riskScoringService = riskScoringService;
        }

        [HttpGet]
        public async Task<ActionResult<RiskAssessmentResult>> Test(
            CancellationToken cancellationToken)
        {
            var context = new ClaimRiskContext
            {
                ClaimId = "CLAIM-ML-TEST-001",
                CorrelationId = HttpContext.TraceIdentifier,

                ClaimAmount = 2400m,
                Units = 8,
                UnitPrice = 300m,
                ServiceCategory = "Therapeutic Supports",
                DaysServiceToSubmission = 4,

                PlanTotalBudget = 80000m,
                PlanRemainingBefore = 7000m,
                PlanUtilisationBefore = 0.9125m,
                ClaimToRemainingRatio = 2400m / 7000m,

                ParticipantClaims7d = 4,
                ParticipantClaims30d = 8,
                ParticipantClaims90d = 14,
                ParticipantAvgClaimPrior = 650m,
                ClaimToParticipantAvg = 2400m / 650m,
                DaysSinceParticipantPreviousClaim = 2,

                ProviderClaims7d = 20,
                ProviderClaims30d = 55,
                ProviderClaims90d = 130,
                ProviderAvgClaimPrior = 780m,
                ClaimToProviderAvg = 2400m / 780m,

                ValidationFindingCount = 3,
                ValidationHighCount = 1,
                DuplicateWarning = false,
                PlanLimitWarning = false
            };

            var result =
                await _riskScoringService.ScoreAsync(
                    context,
                    cancellationToken);

            return Ok(result);
        }
    }
}
