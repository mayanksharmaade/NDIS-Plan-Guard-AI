namespace NDIS.Application.BudgetPlanTemplates.Models;

public sealed record BudgetPlanTemplateDto(
    Guid Id,
    string PlanName,
    string BudgetType,
    decimal Amount,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record SaveBudgetPlanTemplateRequest(
    string PlanName,
    string BudgetType,
    decimal Amount,
    bool IsActive);
