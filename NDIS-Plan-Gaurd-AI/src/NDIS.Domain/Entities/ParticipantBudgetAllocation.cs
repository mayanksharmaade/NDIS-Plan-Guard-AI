namespace NDIS.Domain.Entities;

public sealed class ParticipantBudgetAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParticipantBudgetPlanId { get; set; }

    public string SupportCategory { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal? FortnightLimit { get; set; }

    public ParticipantBudgetPlan ParticipantBudgetPlan { get; set; } = null!;
}
