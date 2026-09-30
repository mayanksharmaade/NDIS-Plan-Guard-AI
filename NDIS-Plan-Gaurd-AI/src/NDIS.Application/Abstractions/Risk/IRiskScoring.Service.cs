using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Abstractions.Risk
{
    public interface IRiskScoringService
    {
        Task<RiskAssessmentResult> ScoreAsync(
            ClaimRiskContext context,
            CancellationToken cancellationToken = default);
    }
}
