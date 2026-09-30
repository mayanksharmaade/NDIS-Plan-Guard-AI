using NDIS.Domain.Common;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Entities;

public sealed class BudgetPlanTemplate : AuditableEntity
{
    private BudgetPlanTemplate() { }

    public BudgetPlanTemplate(
        string planName,
        BudgetType budgetType,
        decimal amount,
        bool isActive = true)
    {
        Update(planName, budgetType, amount, isActive);
    }

    public string PlanName { get; private set; } = string.Empty;
    public BudgetType BudgetType { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<ParticipantBudgetPlan> ParticipantPlans { get; private set; }
        = new List<ParticipantBudgetPlan>();

    public void Update(string planName, BudgetType budgetType, decimal amount, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new ArgumentException("Plan name is required.", nameof(planName));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Plan amount must be greater than zero.");

        PlanName = planName.Trim();
        BudgetType = budgetType;
        Amount = amount;
        IsActive = isActive;
        MarkUpdated();
    }
}
