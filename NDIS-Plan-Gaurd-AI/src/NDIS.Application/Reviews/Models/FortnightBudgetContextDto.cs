using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews.Models
{
    public sealed record FortnightBudgetContextDto
    {
        public DateOnly? FortnightStart { get; init; }

        public DateOnly? FortnightEnd { get; init; }

        public decimal? FortnightBudget { get; init; }

        public decimal FortnightSpentBefore { get; init; }

        public decimal? FortnightRemainingBefore { get; init; }

        public decimal CurrentClaimAmount { get; init; }

        public decimal? RemainingAfterClaim { get; init; }

        public bool BudgetExceeded { get; init; }

        public decimal? AmountOverBudget { get; init; }
    }
}
