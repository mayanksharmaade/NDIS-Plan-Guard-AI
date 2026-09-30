namespace NDIS.Application.BudgetPlans.Models;

public sealed record ParticipantBudgetAllocationDto(
    Guid Id,
    string SupportCategory,
    decimal AllocatedAmount,
    decimal? FortnightLimit);

public sealed record ParticipantBudgetPlanDto(
    Guid Id,
    Guid ParticipantId,
    Guid? BudgetPlanTemplateId,
    string PlanName,
    string BudgetType,
    decimal Amount,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal? TotalBudget,
    decimal? DefaultFortnightBudget,
    string Status,
    IReadOnlyCollection<ParticipantBudgetAllocationDto> Allocations);

public sealed record SaveParticipantBudgetPlanRequest(
    Guid BudgetPlanTemplateId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

public sealed record AddBudgetAllocationRequest(
    string SupportCategory,
    decimal AllocatedAmount,
    decimal? FortnightLimit);
