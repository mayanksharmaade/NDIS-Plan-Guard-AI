using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews
{
    public static class DecisionSupportReasonCodes
    {
        public const string FortnightBudgetExceeded =
            "FORTNIGHT_BUDGET_EXCEEDED";

        public const string DuplicateService =
            "DUPLICATE_SERVICE";

        public const string RateMismatch =
            "RATE_MISMATCH";

        public const string AmbiguousRate =
            "AMBIGUOUS_RATE";

        public const string AmountMismatch =
            "AMOUNT_MISMATCH";

        public const string ExcessiveServiceHours =
            "EXCESSIVE_SERVICE_HOURS";

        public const string OverlappingService =
            "OVERLAPPING_SERVICE";

        public const string ConcurrentProviderService =
            "CONCURRENT_PROVIDER_SERVICE";

        public const string ImpossibleLocationOverlap =
            "IMPOSSIBLE_LOCATION_OVERLAP";
    }
}
