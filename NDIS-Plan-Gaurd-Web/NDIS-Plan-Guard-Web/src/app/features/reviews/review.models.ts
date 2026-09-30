export interface ReviewQueueItemResponse {
  claimId: string;
  claimNumber: string;
  serviceProviderId: string;
  serviceProviderName: string;
  participantId: string;
  participantName: string;
  participantNdisNumber: string;
  serviceFrom: string;
  serviceTo: string;
  supportCategory: string;
  amount: number;
  status: string;
  submittedAtUtc: string | null;
  reviewStartedAtUtc: string | null;
}

export interface ClaimReviewHistoryItemResponse {
  id: string;
  reviewerIdentityUserId: string;
  reviewerEmail: string;
  outcome: string;
  comments: string | null;
  reviewedAtUtc: string;
}

export interface ClaimForReviewResponse {
  claimId: string;
  claimNumber: string;
  serviceProviderId: string;
  serviceProviderName: string;
  participantId: string;
  participantName: string;
  participantNdisNumber: string;
  serviceFrom: string;
  serviceTo: string;
  supportCategory: string;
  description: string;
  amount: number;
  status: string;
  createdAtUtc: string;
  submittedAtUtc: string | null;
  reviewStartedAtUtc: string | null;
  decidedAtUtc: string | null;
  reviews: ClaimReviewHistoryItemResponse[];
}

export type ClaimReviewDecision = 'Approved' | 'Rejected' | 'MoreInformationRequired';

export interface RecordClaimDecisionRequest {
  decision: ClaimReviewDecision;
  comments: string | null;
}
