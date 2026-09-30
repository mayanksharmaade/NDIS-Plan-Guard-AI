using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;
using NDIS.Application.BudgetPlans.Models;
using NDIS.Domain.Common;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.BudgetPlans;

public sealed class ParticipantBudgetPlanService : IParticipantBudgetPlanService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ParticipantBudgetPlanService(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ParticipantBudgetPlanDto>> GetByParticipantAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        var providerId = await GetCurrentProviderIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active Service Provider membership was not found.");

        var participantBelongsToProvider = await _db.Participants.AsNoTracking().AnyAsync(
            x => x.Id == participantId && x.ServiceProviderId == providerId,
            cancellationToken);

        if (!participantBelongsToProvider)
            throw new InvalidOperationException("Participant was not found for this Service Provider.");

        var plans = await _db.ParticipantBudgetPlans
            .AsNoTracking()
            .Include(x => x.BudgetPlanTemplate)
            .Include(x => x.Allocations)
            .Where(x => x.ParticipantId == participantId)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync(cancellationToken);

        return plans.Select(ToDto).ToList();
    }

    public async Task<ParticipantBudgetPlanDto> CreateAsync(
        Guid participantId,
        SaveParticipantBudgetPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.EndDate < request.StartDate)
            throw new InvalidOperationException("End date cannot be before start date.");

        if (!Enum.TryParse<BudgetPlanStatus>(request.Status, true, out var status))
            throw new InvalidOperationException("Invalid budget plan status.");

        var providerId = await GetCurrentProviderIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Active Service Provider membership was not found.");

        var participantExists = await _db.Participants.AnyAsync(
            x => x.Id == participantId && x.ServiceProviderId == providerId,
            cancellationToken);

        if (!participantExists)
            throw new InvalidOperationException("Participant was not found for this Service Provider.");

        var template = await _db.BudgetPlanTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.BudgetPlanTemplateId && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException("Active budget plan template not found.");

        var overlaps = await _db.ParticipantBudgetPlans.AnyAsync(
            x => x.ParticipantId == participantId
                 && x.Status == BudgetPlanStatus.Active
                 && request.StartDate <= x.EndDate
                 && request.EndDate >= x.StartDate,
            cancellationToken);

        if (status == BudgetPlanStatus.Active && overlaps)
            throw new InvalidOperationException("The participant already has an active budget plan overlapping these dates.");

        var entity = new ParticipantBudgetPlan
        {
            ParticipantId = participantId,
            BudgetPlanTemplateId = template.Id,
            PlanName = template.PlanName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalBudget = template.BudgetType == BudgetType.TotalPlan ? template.Amount : null,
            DefaultFortnightBudget = template.BudgetType == BudgetType.Fortnightly ? template.Amount : null,
            Status = status
        };

        _db.ParticipantBudgetPlans.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        entity.BudgetPlanTemplate = template;
        return ToDto(entity);
    }

    public async Task<ParticipantBudgetAllocationDto> AddAllocationAsync(
        Guid participantId,
        Guid budgetPlanId,
        AddBudgetAllocationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportServiceCatalog.IsValid(request.SupportCategory))
            throw new InvalidOperationException("Select a valid support service.");

        if (request.AllocatedAmount <= 0)
            throw new InvalidOperationException("Allocated amount must be greater than zero.");

        if (request.FortnightLimit.HasValue && request.FortnightLimit.Value <= 0)
            throw new InvalidOperationException("Fortnight limit must be greater than zero when supplied.");

        var plan = await _db.ParticipantBudgetPlans
            .Include(x => x.Allocations)
            .SingleOrDefaultAsync(
                x => x.Id == budgetPlanId && x.ParticipantId == participantId,
                cancellationToken)
            ?? throw new InvalidOperationException("Budget plan not found.");

        if (plan.TotalBudget.HasValue)
        {
            var currentlyAllocated = plan.Allocations.Sum(x => x.AllocatedAmount);
            if (currentlyAllocated + request.AllocatedAmount > plan.TotalBudget.Value)
                throw new InvalidOperationException("Category allocations cannot exceed total budget.");
        }

        var code = SupportServiceCatalog.Normalize(request.SupportCategory);
        var duplicate = plan.Allocations.Any(x => x.SupportCategory == code);
        if (duplicate)
            throw new InvalidOperationException("This support service already has an allocation on the plan.");

        var allocation = new ParticipantBudgetAllocation
        {
            ParticipantBudgetPlanId = plan.Id,
            SupportCategory = code,
            AllocatedAmount = request.AllocatedAmount,
            FortnightLimit = request.FortnightLimit
        };

        _db.ParticipantBudgetAllocations.Add(allocation);
        await _db.SaveChangesAsync(cancellationToken);

        return new ParticipantBudgetAllocationDto(
            allocation.Id,
            allocation.SupportCategory,
            allocation.AllocatedAmount,
            allocation.FortnightLimit);
    }

    private static ParticipantBudgetPlanDto ToDto(ParticipantBudgetPlan x)
    {
        var budgetType = x.BudgetPlanTemplate?.BudgetType.ToString()
            ?? (x.DefaultFortnightBudget.HasValue ? BudgetType.Fortnightly.ToString() : BudgetType.TotalPlan.ToString());

        var amount = x.DefaultFortnightBudget ?? x.TotalBudget ?? 0m;

        return new ParticipantBudgetPlanDto(
            x.Id,
            x.ParticipantId,
            x.BudgetPlanTemplateId,
            x.PlanName,
            budgetType,
            amount,
            x.StartDate,
            x.EndDate,
            x.TotalBudget,
            x.DefaultFortnightBudget,
            x.Status.ToString(),
            x.Allocations
                .Select(a => new ParticipantBudgetAllocationDto(
                    a.Id,
                    a.SupportCategory,
                    a.AllocatedAmount,
                    a.FortnightLimit))
                .ToList());
    }

    private async Task<Guid?> GetCurrentProviderIdAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid identityUserId)
            return null;

        return await CurrentProviderResolver.ResolveAsync(
            _db,
            identityUserId,
            cancellationToken);
    }
}
