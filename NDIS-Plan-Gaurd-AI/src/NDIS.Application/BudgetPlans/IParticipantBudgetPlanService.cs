using NDIS.Application.BudgetPlans.Models;

namespace NDIS.Application.BudgetPlans;

public interface IParticipantBudgetPlanService
{
    Task<IReadOnlyCollection<ParticipantBudgetPlanDto>> GetByParticipantAsync(
        Guid participantId,
        CancellationToken cancellationToken = default);

    Task<ParticipantBudgetPlanDto> CreateAsync(
        Guid participantId,
        SaveParticipantBudgetPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<ParticipantBudgetAllocationDto> AddAllocationAsync(
        Guid participantId,
        Guid budgetPlanId,
        AddBudgetAllocationRequest request,
        CancellationToken cancellationToken = default);
}
