using NDIS.Application.Reviews.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews
{
    public static class DecisionSupportFindingBuilder
    {
        public static IReadOnlyCollection<DecisionSupportFindingDto> Build(
            FortnightBudgetContextDto budget,
            ServiceCostContextDto service)
        {
            var findings =
                new List<DecisionSupportFindingDto>();

            if (budget.BudgetExceeded)
            {
                findings.Add(
                    new DecisionSupportFindingDto
                    {
                        Code =
                            DecisionSupportReasonCodes
                                .FortnightBudgetExceeded,

                        Title =
                            "Fortnight budget exceeded",

                        Description =
                            "The current claim exceeds the participant's remaining fortnight allocation.",

                        Severity = "High",

                        Evidence =
                            $"Remaining: {budget.FortnightRemainingBefore:C}. " +
                            $"Claim: {budget.CurrentClaimAmount:C}."
                    });
            }

            if (service.ExpectedServiceCost.HasValue
                && Math.Abs(
                    service.ClaimedAmount
                    - service.ExpectedServiceCost.Value) > 1m)
            {
                findings.Add(
                    new DecisionSupportFindingDto
                    {
                        Code =
                            DecisionSupportReasonCodes
                                .AmountMismatch,

                        Title =
                            "Claimed amount differs from expected service cost",

                        Description =
                            "The submitted amount does not match the expected cost calculated from rate and hours.",

                        Severity = "Medium",

                        Evidence =
                            $"Expected: {service.ExpectedServiceCost:C}. " +
                            $"Claimed: {service.ClaimedAmount:C}."
                    });
            }

            if (service.RateVariancePercent.HasValue
                && Math.Abs(service.RateVariancePercent.Value) >= 20m)
            {
                findings.Add(
                    new DecisionSupportFindingDto
                    {
                        Code =
                            DecisionSupportReasonCodes.RateMismatch,

                        Title =
                            "Provider rate requires review",

                        Description =
                            "The current hourly rate differs materially from the provider's typical rate.",

                        Severity = "Medium",

                        Evidence =
                            $"Current rate: {service.ClaimedHourlyRate:C}. " +
                            $"Typical rate: {service.ProviderTypicalHourlyRate:C}. " +
                            $"Variance: {service.RateVariancePercent:N1}%."
                    });
            }

            if (service.ConcurrentParticipantCount > 1)
            {
                findings.Add(
                    new DecisionSupportFindingDto
                    {
                        Code =
                            DecisionSupportReasonCodes
                                .ConcurrentProviderService,

                        Title =
                            "Concurrent participant services detected",

                        Description =
                            "The same provider appears to have delivered overlapping services to multiple participants.",

                        Severity = "High",

                        Evidence =
                            $"{service.ConcurrentParticipantCount} participants have overlapping service periods."
                    });
            }

            if (service.OverlappingServiceMinutes > 0)
            {
                findings.Add(
                    new DecisionSupportFindingDto
                    {
                        Code =
                            DecisionSupportReasonCodes
                                .OverlappingService,

                        Title =
                            "Overlapping service window",

                        Description =
                            "Another service claim overlaps with this service period.",

                        Severity = "High",

                        Evidence =
                            $"{service.OverlappingServiceMinutes} overlapping minutes detected."
                    });
            }

            return findings;
        }
    }
    }
