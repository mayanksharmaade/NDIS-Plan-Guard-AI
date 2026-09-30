export interface DashboardRecentClaimResponse {
  claimId: string;
  claimNumber: string;
  participantName: string;
  serviceProviderName: string | null;
  amount: number;
  status: string;
  createdAtUtc: string;
  submittedAtUtc: string | null;
}

export interface DashboardSummaryResponse {
  scope: string;
  totalParticipants: number;
  activeParticipants: number;
  totalClaims: number;
  draftClaims: number;
  submittedClaims: number;
  inReviewClaims: number;
  moreInformationRequiredClaims: number;
  approvedClaims: number;
  rejectedClaims: number;
  totalClaimAmount: number;
  approvedAmount: number;
  pendingProviderRegistrations: number;
  activeServiceProviders: number;
  recentClaims: DashboardRecentClaimResponse[];
}
