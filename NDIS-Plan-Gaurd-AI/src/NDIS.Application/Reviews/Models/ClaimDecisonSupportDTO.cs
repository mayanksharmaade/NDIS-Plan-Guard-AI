using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews.Models
{
    public sealed record ClaimDecisionSupportDto
    {
        public Guid ClaimId { get; init; }

        public required string ClaimNumber { get; init; }

        public required string Status { get; init; }

        public decimal ClaimAmount { get; init; }

        public required string ProviderName { get; init; }

        public required string SupportCategory { get; init; }

        public DateTimeOffset? SubmittedAtUtc { get; init; }

        public required ParticipantSupportContextDto Participant { get; init; }

        public required FortnightBudgetContextDto Budget { get; init; }

        public required ServiceCostContextDto Service { get; init; }

        public IReadOnlyCollection<DecisionSupportFindingDto> Findings { get; init; }
            = Array.Empty<DecisionSupportFindingDto>();

        public MlDecisionSupportDto? MlRisk { get; init; }

        public IReadOnlyCollection<ReviewHistoryItemDto> ReviewHistory { get; init; }
            = Array.Empty<ReviewHistoryItemDto>();
    }
}
