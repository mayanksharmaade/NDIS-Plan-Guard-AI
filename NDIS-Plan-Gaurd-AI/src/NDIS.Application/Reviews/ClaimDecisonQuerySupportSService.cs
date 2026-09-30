using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Reviews.Models;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Reviews;

public sealed class ClaimDecisionSupportQueryService : IClaimDecisionSupportQueryService
{
    private readonly IAppDbContext _dbContext;

    public ClaimDecisionSupportQueryService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClaimDecisionSupportDto?> GetAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .Include(x => x.ServiceProvider)
            .SingleOrDefaultAsync(x => x.Id == claimId, cancellationToken);

        if (claim is null)
            return null;

        var reviews = await _dbContext.ClaimReviews
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId)
            .OrderByDescending(x => x.ReviewedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Outcome,
                x.ReviewerEmail,
                x.Comments,
                x.ReviewedAtUtc
            })
            .ToListAsync(cancellationToken);

        var latestRisk = await _dbContext.ClaimRiskAssessments
            .AsNoTracking()
            .Include(x => x.Factors)
            .Where(x => x.ClaimId == claimId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var deliveryEvidence = await _dbContext.ClaimServiceDeliveries
            .AsNoTracking()
            .Where(x => x.ClaimId == claimId)
            .Select(x => new
            {
                x.ServiceDelivery.ServiceProviderEmployeeId,
                EmployeeName = x.ServiceDelivery.ServiceProviderEmployee.FirstName + " "
                    + x.ServiceDelivery.ServiceProviderEmployee.LastName,
                EmployeePayRate = x.ServiceDelivery.ServiceProviderEmployee.DefaultHourlyRate,
                x.ServiceDelivery.ServiceHours,
                x.ServiceDelivery.HourlyRate,
                x.ServiceDelivery.Amount,
                x.ServiceDelivery.ServiceLocation,
                x.ServiceDelivery.ServiceStartUtc,
                x.ServiceDelivery.ServiceEndUtc
            })
            .ToListAsync(cancellationToken);

        var priorProviderRates = await _dbContext.Claims
            .AsNoTracking()
            .Where(x =>
                x.Id != claim.Id
                && x.ServiceProviderId == claim.ServiceProviderId
                && x.SupportCategory == claim.SupportCategory
                && (x.AgreedHourlyRate.HasValue || x.UnitPrice.HasValue)
                && (!claim.SubmittedAtUtc.HasValue
                    || !x.SubmittedAtUtc.HasValue
                    || x.SubmittedAtUtc < claim.SubmittedAtUtc))
            .Select(x => x.AgreedHourlyRate ?? x.UnitPrice!.Value)
            .ToListAsync(cancellationToken);

        var serviceRates = await _dbContext.Claims
            .AsNoTracking()
            .Where(x =>
                x.Id != claim.Id
                && x.SupportCategory == claim.SupportCategory
                && (x.AgreedHourlyRate.HasValue || x.UnitPrice.HasValue)
                && (!claim.SubmittedAtUtc.HasValue
                    || !x.SubmittedAtUtc.HasValue
                    || x.SubmittedAtUtc < claim.SubmittedAtUtc))
            .Select(x => x.AgreedHourlyRate ?? x.UnitPrice!.Value)
            .ToListAsync(cancellationToken);

        decimal? providerTypicalRate = priorProviderRates.Count == 0
            ? null
            : priorProviderRates.Average();

        decimal? serviceTypicalRate = serviceRates.Count == 0
            ? null
            : serviceRates.Average();

        decimal? deliveryHours = deliveryEvidence.Count > 0
    ? deliveryEvidence.Sum(x => x.ServiceHours)
    : null;

        decimal? deliveryRate = null;

        if (deliveryEvidence.Count > 0)
        {
            var distinctRates = deliveryEvidence
                .Select(x => x.HourlyRate)
                .Distinct()
                .ToList();

            if (distinctRates.Count == 1)
            {
                deliveryRate = distinctRates[0];
            }
        }

        decimal? serviceHours =
            claim.TotalServiceHours
            ?? deliveryHours
            ?? (claim.Units.HasValue
                ? (decimal)claim.Units.Value
                : null);

        decimal? claimedRate =
            claim.AgreedHourlyRate
            ?? deliveryRate
            ?? claim.UnitPrice;

        decimal? expectedServiceCost = serviceHours.HasValue && claimedRate.HasValue
            ? Math.Round(serviceHours.Value * claimedRate.Value, 2)
            : null;

        decimal? amountVariance = expectedServiceCost.HasValue
            ? claim.Amount - expectedServiceCost.Value
            : null;

        decimal? rateVariancePercent = null;
        if (claimedRate.HasValue && providerTypicalRate.HasValue && providerTypicalRate.Value > 0)
        {
            rateVariancePercent =
                ((claimedRate.Value - providerTypicalRate.Value) / providerTypicalRate.Value) * 100m;
        }

        var budget = await BuildBudgetContextAsync(claim, cancellationToken);

        var distinctLocations = deliveryEvidence
            .Select(x => x.ServiceLocation)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var employeeNames = deliveryEvidence
            .Select(x => x.EmployeeName)
            .Distinct()
            .ToArray();

        var employeePayRates = deliveryEvidence
            .Where(x => x.EmployeePayRate.HasValue)
            .Select(x => x.EmployeePayRate!.Value)
            .Distinct()
            .ToArray();

        var service = new ServiceCostContextDto
        {
            EmployeeName = employeeNames.Length switch
            {
                0 => null,
                1 => employeeNames[0],
                _ => "Multiple support workers"
            },
            DeliveryCount = deliveryEvidence.Count,
            EmployeePayRate = employeePayRates.Length == 1 ? employeePayRates[0] : null,
            ServiceHours = serviceHours,
            ClaimedHourlyRate = claimedRate,
            ProviderTypicalHourlyRate = providerTypicalRate,
            ServiceTypicalHourlyRate = serviceTypicalRate,
            ExpectedServiceCost = expectedServiceCost,
            ClaimedAmount = claim.Amount,
            AmountVariance = amountVariance,
            RateVariancePercent = rateVariancePercent,
            ServiceLocation = distinctLocations.Length switch
            {
                0 => null,
                1 => distinctLocations[0],
                _ => "Multiple locations"
            },
            ConcurrentParticipantCount = 0,
            OverlappingServiceMinutes = 0
        };

        var findings = DecisionSupportFindingBuilder.Build(budget, service);

        var participant = new ParticipantSupportContextDto
        {
            ParticipantName = ParticipantName(claim.Participant),
            NdisNumber = claim.Participant.NdisNumber,
            PrimaryDisabilityCategory = null,
            FunctionalSupportDomains = Array.Empty<string>(),
            ApprovedSupportCategories = Array.Empty<string>(),
            TypicalSupportHoursPerFortnight = null,
            SupportIntensity = null,
            SpecialSupportRequirements = null
        };

        var reviewHistory = reviews
            .Select(x => new ReviewHistoryItemDto
            {
                Id = x.Id,
                Decision = x.Outcome.ToString(),
                ReviewerName = null,
                ReviewerEmail = x.ReviewerEmail,
                Comments = x.Comments,
                ReviewedAtUtc = ToDateTimeOffset(x.ReviewedAtUtc)
            })
            .ToArray();

        MlDecisionSupportDto? mlRisk = null;
        if (latestRisk is not null)
        {
            mlRisk = new MlDecisionSupportDto
            {
                Available = latestRisk.Available,
                Probability = latestRisk.Probability,
                Score = latestRisk.Score,
                RiskBand = latestRisk.RiskBand,
                ModelVersion = latestRisk.ModelVersion,
                FeatureVersion = latestRisk.FeatureVersion,
                ScoredAtUtc = latestRisk.ScoredAtUtc,
                FailureReason = latestRisk.FailureReason,
                Factors = latestRisk.Factors
                    .OrderByDescending(x => Math.Abs(x.ProbabilityImpact))
                    .Select(x => new MlRiskFactorDto
                    {
                        Feature = x.Feature,
                        Description = x.Description,
                        ObservedValue = x.ObservedValueJson,
                        BaselineValue = x.BaselineValueJson,
                        ProbabilityImpact = x.ProbabilityImpact
                    })
                    .ToArray()
            };
        }

        return new ClaimDecisionSupportDto
        {
            ClaimId = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            Status = claim.Status.ToString(),
            ClaimAmount = claim.Amount,
            ProviderName = ProviderName(claim.ServiceProvider),
            SupportCategory = claim.SupportCategory,
            SubmittedAtUtc = claim.SubmittedAtUtc.HasValue
                ? ToDateTimeOffset(claim.SubmittedAtUtc.Value)
                : null,
            Participant = participant,
            Budget = budget,
            Service = service,
            Findings = findings,
            MlRisk = mlRisk,
            ReviewHistory = reviewHistory
        };
    }

    private async Task<FortnightBudgetContextDto> BuildBudgetContextAsync(
        Claim claim,
        CancellationToken cancellationToken)
    {
        var claimDate = DateOnly.FromDateTime(claim.ServiceFrom.Date);

        var plan = await _dbContext.ParticipantBudgetPlans
            .AsNoTracking()
            .Where(x =>
                x.ParticipantId == claim.ParticipantId
                && x.Status == BudgetPlanStatus.Active
                && x.StartDate <= claimDate
                && x.EndDate >= claimDate)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan?.DefaultFortnightBudget is not decimal fortnightBudget || fortnightBudget <= 0)
        {
            return new FortnightBudgetContextDto
            {
                CurrentClaimAmount = claim.Amount,
                FortnightSpentBefore = 0m,
                BudgetExceeded = false
            };
        }

        var dayOffset = Math.Max(0, claimDate.DayNumber - plan.StartDate.DayNumber);
        var periodIndex = dayOffset / 14;
        var fortnightStart = plan.StartDate.AddDays(periodIndex * 14);
        var fortnightEnd = fortnightStart.AddDays(13);
        if (fortnightEnd > plan.EndDate)
            fortnightEnd = plan.EndDate;

        var from = fortnightStart.ToDateTime(TimeOnly.MinValue);
        var to = fortnightEnd.ToDateTime(TimeOnly.MaxValue);

        var spentBefore = await _dbContext.Claims
            .AsNoTracking()
            .Where(x =>
                x.ParticipantId == claim.ParticipantId
                && x.Id != claim.Id
                && x.ServiceFrom <= to
                && x.ServiceTo >= from
                && x.Status != ClaimStatus.Draft
                && x.Status != ClaimStatus.Rejected
                && x.Status != ClaimStatus.Cancelled
                && (!claim.SubmittedAtUtc.HasValue
                    || !x.SubmittedAtUtc.HasValue
                    || x.SubmittedAtUtc < claim.SubmittedAtUtc))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var remainingBefore = Math.Max(0m, fortnightBudget - spentBefore);
        var remainingAfter = remainingBefore - claim.Amount;
        var exceeded = remainingAfter < 0;

        return new FortnightBudgetContextDto
        {
            FortnightStart = fortnightStart,
            FortnightEnd = fortnightEnd,
            FortnightBudget = fortnightBudget,
            FortnightSpentBefore = spentBefore,
            FortnightRemainingBefore = remainingBefore,
            CurrentClaimAmount = claim.Amount,
            RemainingAfterClaim = remainingAfter,
            BudgetExceeded = exceeded,
            AmountOverBudget = exceeded ? Math.Abs(remainingAfter) : 0m
        };
    }

    private static string ProviderName(ServiceProvider provider) =>
        string.IsNullOrWhiteSpace(provider.TradingName)
            ? provider.LegalName
            : provider.TradingName;

    private static string ParticipantName(Participant participant) =>
        $"{participant.FirstName} {participant.LastName}".Trim();

    private static DateTimeOffset ToDateTimeOffset(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return new DateTimeOffset(utc);
    }
}
