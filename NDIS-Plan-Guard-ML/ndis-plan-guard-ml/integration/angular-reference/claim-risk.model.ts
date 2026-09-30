export interface ClaimRiskFactor {
  feature: string;
  description: string;
  observedValue: unknown;
  baselineValue: unknown;
  probabilityImpact: number;
}

export interface ClaimRiskResult {
  claimId: string;
  riskProbability: number;
  riskScore: number;
  riskBand: 'Low' | 'Medium' | 'High' | string;
  topFactors: ClaimRiskFactor[];
  modelVersion: string;
  featureVersion: string;
  scoredAtUtc: string;
  decisionSupportNotice: string;
}
