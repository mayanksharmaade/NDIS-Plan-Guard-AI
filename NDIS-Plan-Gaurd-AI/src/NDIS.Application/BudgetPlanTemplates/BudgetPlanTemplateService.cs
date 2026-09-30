using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.BudgetPlanTemplates.Models;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.BudgetPlanTemplates;

public sealed class BudgetPlanTemplateService : IBudgetPlanTemplateService
{
    private readonly IAppDbContext _db;

    public BudgetPlanTemplateService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<BudgetPlanTemplateDto>> GetAllAsync(
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _db.BudgetPlanTemplates.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.PlanName)
            .Select(x => new BudgetPlanTemplateDto(
                x.Id,
                x.PlanName,
                x.BudgetType.ToString(),
                x.Amount,
                x.IsActive,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<BudgetPlanTemplateDto> CreateAsync(
        SaveBudgetPlanTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var budgetType = ParseBudgetType(request.BudgetType);
        Validate(request.PlanName, request.Amount);

        var name = request.PlanName.Trim();
        var duplicate = await _db.BudgetPlanTemplates.AnyAsync(
            x => x.PlanName == name,
            cancellationToken);

        if (duplicate)
            throw new InvalidOperationException("A budget plan template with this name already exists.");

        var entity = new BudgetPlanTemplate(
            name,
            budgetType,
            request.Amount,
            request.IsActive);

        _db.BudgetPlanTemplates.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<BudgetPlanTemplateDto?> UpdateAsync(
        Guid id,
        SaveBudgetPlanTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BudgetPlanTemplates.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);

        if (entity is null)
            return null;

        var budgetType = ParseBudgetType(request.BudgetType);
        Validate(request.PlanName, request.Amount);
        var name = request.PlanName.Trim();

        var duplicate = await _db.BudgetPlanTemplates.AnyAsync(
            x => x.Id != id && x.PlanName == name,
            cancellationToken);

        if (duplicate)
            throw new InvalidOperationException("A budget plan template with this name already exists.");

        entity.Update(name, budgetType, request.Amount, request.IsActive);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    private static BudgetType ParseBudgetType(string value)
    {
        if (!Enum.TryParse<BudgetType>(value, true, out var parsed))
            throw new InvalidOperationException("Budget type must be TotalPlan or Fortnightly.");

        return parsed;
    }

    private static void Validate(string planName, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new InvalidOperationException("Plan name is required.");

        if (planName.Trim().Length > 150)
            throw new InvalidOperationException("Plan name cannot exceed 150 characters.");

        if (amount <= 0)
            throw new InvalidOperationException("Plan amount must be greater than zero.");
    }

    private static BudgetPlanTemplateDto ToDto(BudgetPlanTemplate x) =>
        new(
            x.Id,
            x.PlanName,
            x.BudgetType.ToString(),
            x.Amount,
            x.IsActive,
            x.CreatedAtUtc,
            x.UpdatedAtUtc);
}
