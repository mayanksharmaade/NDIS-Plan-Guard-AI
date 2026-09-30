using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class ParticipantBudgetPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParticipantId { get; set; }
    public Guid? BudgetPlanTemplateId { get; set; }

    // Snapshot fields keep the participant's assigned plan stable even if the
    // master template is later edited.
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? TotalBudget { get; set; }
    public decimal? DefaultFortnightBudget { get; set; }
    public BudgetPlanStatus Status { get; set; } = BudgetPlanStatus.Draft;

    public Participant Participant { get; set; } = null!;
    public BudgetPlanTemplate? BudgetPlanTemplate { get; set; }

    public ICollection<ParticipantBudgetAllocation> Allocations { get; set; }
        = new List<ParticipantBudgetAllocation>();
}
