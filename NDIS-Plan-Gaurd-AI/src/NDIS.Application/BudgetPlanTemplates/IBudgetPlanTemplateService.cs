using NDIS.Application.BudgetPlanTemplates.Models;

namespace NDIS.Application.BudgetPlanTemplates;

public interface IBudgetPlanTemplateService
{
    Task<IReadOnlyCollection<BudgetPlanTemplateDto>> GetAllAsync(
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task<BudgetPlanTemplateDto> CreateAsync(
        SaveBudgetPlanTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<BudgetPlanTemplateDto?> UpdateAsync(
        Guid id,
        SaveBudgetPlanTemplateRequest request,
        CancellationToken cancellationToken = default);
}
