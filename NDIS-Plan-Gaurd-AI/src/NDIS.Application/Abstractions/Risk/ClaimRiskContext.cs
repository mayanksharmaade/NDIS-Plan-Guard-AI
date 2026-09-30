using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Abstractions.Risk;

public sealed record ClaimRiskContext
{
    public required string ClaimId { get; init; }

    public string? CorrelationId { get; init; }

    // Current claim
    public decimal ClaimAmount { get; init; }

    public int Units { get; init; }

    public decimal UnitPrice { get; init; }

    public required string ServiceCategory { get; init; }

    public int DaysServiceToSubmission { get; init; }

    // Plan
    public decimal PlanTotalBudget { get; init; }

    public decimal PlanRemainingBefore { get; init; }

    public decimal PlanUtilisationBefore { get; init; }

    public decimal ClaimToRemainingRatio { get; init; }

    // Participant history
    public int ParticipantClaims7d { get; init; }

    public int ParticipantClaims30d { get; init; }

    public int ParticipantClaims90d { get; init; }

    public decimal? ParticipantAvgClaimPrior { get; init; }

    public decimal ClaimToParticipantAvg { get; init; }

    public int? DaysSinceParticipantPreviousClaim { get; init; }

    // Provider history
    public int ProviderClaims7d { get; init; }

    public int ProviderClaims30d { get; init; }

    public int ProviderClaims90d { get; init; }

    public decimal? ProviderAvgClaimPrior { get; init; }

    public decimal ClaimToProviderAvg { get; init; }

    // Deterministic validation
    public int ValidationFindingCount { get; init; }

    public int ValidationHighCount { get; init; }

    public bool DuplicateWarning { get; init; }

    public bool PlanLimitWarning { get; init; }
}
