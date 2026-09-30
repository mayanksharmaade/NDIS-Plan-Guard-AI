export interface BudgetPlanTemplate {
  id: string;
  planName: string;
  budgetType: 'TotalPlan' | 'Fortnightly' | string;
  amount: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface SaveBudgetPlanTemplateRequest {
  planName: string;
  budgetType: 'TotalPlan' | 'Fortnightly';
  amount: number;
  isActive: boolean;
}
