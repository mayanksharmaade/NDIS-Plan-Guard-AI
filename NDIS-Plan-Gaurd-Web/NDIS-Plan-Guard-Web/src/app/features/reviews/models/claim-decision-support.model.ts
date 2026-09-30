export interface ClaimDecisionSupport {
  claimId: string;
  claimNumber: string;

  status: string;

  claimAmount: number;

  providerName: string;

  supportCategory: string;

  submittedAtUtc?: string;

  participant: ParticipantSupportContext;

  budget: FortnightBudgetContext;

  service: ServiceCostContext;

  findings: DecisionSupportFinding[];

  mlRisk?: MlDecisionSupport;
}

export interface ParticipantSupportContext {
  participantName: string;

  ndisNumber: string;

  primaryDisabilityCategory?: string;

  functionalSupportDomains: string[];

  approvedSupportCategories: string[];

  typicalSupportHoursPerFortnight?: number;

  supportIntensity?: string;

  specialSupportRequirements?: string;
}

export interface FortnightBudgetContext {
  fortnightStart?: string;

  fortnightEnd?: string;

  fortnightBudget?: number;

  fortnightSpentBefore: number;

  fortnightRemainingBefore?: number;

  currentClaimAmount: number;

  remainingAfterClaim?: number;

  budgetExceeded: boolean;

  amountOverBudget?: number;
}

export interface ServiceCostContext {
  employeeName?: string;

  deliveryCount: number;

  employeePayRate?: number;

  serviceHours?: number;

  claimedHourlyRate?: number;

  providerTypicalHourlyRate?: number;

  serviceTypicalHourlyRate?: number;

  expectedServiceCost?: number;

  claimedAmount: number;

  amountVariance?: number;

  rateVariancePercent?: number;

  serviceLocation?: string;

  concurrentParticipantCount: number;

  overlappingServiceMinutes: number;
}

export interface DecisionSupportFinding {
  code: string;

  title: string;

  description: string;

  severity: 'Low' | 'Medium' | 'High';

  evidence?: string;
}

export interface MlDecisionSupport {
  available: boolean;

  probability?: number;

  score?: number;

  riskBand?: 'Low' | 'Medium' | 'High';

  modelVersion?: string;

  featureVersion?: string;

  scoredAtUtc?: string;

  failureReason?: string;

  factors: MlRiskFactor[];
}

export interface MlRiskFactor {
  feature: string;

  description: string;

  observedValue?: string;

  baselineValue?: string;

  probabilityImpact: number;
}