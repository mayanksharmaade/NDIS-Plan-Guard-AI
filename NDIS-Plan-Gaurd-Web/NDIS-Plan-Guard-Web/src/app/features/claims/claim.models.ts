export interface ClaimListItemResponse {
  id: string;
  claimNumber: string;
  participantId: string;
  participantName: string;
  serviceFrom: string;
  serviceTo: string;
  supportCategory: string;
  amount: number;
  units?: number | null;
  unitPrice?: number | null;
  status: string;
  createdAtUtc: string;
  submittedAtUtc: string | null;
}

export interface ClaimServiceDeliveryResponse {
  id: string;
  serviceProviderEmployeeId: string;
  employeeName: string;
  supportCategory: string;
  serviceStartUtc: string;
  serviceEndUtc: string;
  serviceHours: number;
  hourlyRate: number;
  amount: number;
  serviceLocation: string | null;
}

export interface EligibleServiceDeliveryResponse {
  id: string;
  participantServiceAssignmentId: string;
  serviceProviderEmployeeId: string;
  employeeName: string;
  supportCategory: string;
  serviceStartUtc: string;
  serviceEndUtc: string;
  serviceHours: number;
  agreedHourlyRate: number;
  amount: number;
  serviceLocation: string | null;
}

export interface ClaimDetailsResponse {
  id: string;
  claimNumber: string;
  serviceProviderId: string;
  participantId: string;
  participantNdisNumber: string;
  participantName: string;
  serviceFrom: string;
  serviceTo: string;
  supportCategory: string;
  description: string;
  amount: number;
  units?: number | null;
  unitPrice?: number | null;
  status: string;
  createdAtUtc: string;
  submittedAtUtc: string | null;
  totalServiceHours?: number | null;
  agreedHourlyRate?: number | null;
  employeeName?: string | null;
  serviceDeliveries?: ClaimServiceDeliveryResponse[] | null;
}

export interface CreateClaimRequest {
  participantId: string;
  serviceFrom: string;
  serviceTo: string;
  supportCategory: string;
  description: string;
  amount: number;
  units?: number | null;
  unitPrice?: number | null;
  serviceDeliveryIds: string[];
}

export interface UpdateClaimRequest {
  serviceFrom: string;
  serviceTo: string;
  supportCategory: string;
  description: string;
  amount: number;
  units?: number | null;
  unitPrice?: number | null;
}

export interface ClaimMutationResponse {
  claimId: string;
  claimNumber: string;
  message: string;
}
