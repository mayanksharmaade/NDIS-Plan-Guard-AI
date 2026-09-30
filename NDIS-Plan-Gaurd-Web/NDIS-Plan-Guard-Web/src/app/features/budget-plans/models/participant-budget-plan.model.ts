export interface ParticipantBudgetAllocation {
  id: string;
  supportCategory: string;
  allocatedAmount: number;
  fortnightLimit: number | null;
}

export interface ParticipantBudgetPlan {
  id: string;
  participantId: string;
  budgetPlanTemplateId: string | null;
  planName: string;
  budgetType: 'TotalPlan' | 'Fortnightly' | string;
  amount: number;
  startDate: string;
  endDate: string;
  totalBudget: number | null;
  defaultFortnightBudget: number | null;
  status: string;
  allocations: ParticipantBudgetAllocation[];
}

export interface SaveParticipantBudgetPlanRequest {
  budgetPlanTemplateId: string;
  startDate: string;
  endDate: string;
  status: string;
}
