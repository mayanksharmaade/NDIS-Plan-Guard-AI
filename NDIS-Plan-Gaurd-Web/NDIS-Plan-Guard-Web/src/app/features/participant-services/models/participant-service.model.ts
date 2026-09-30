export interface ParticipantServiceAssignment {
  id: string;
  participantId: string;
  serviceProviderEmployeeId: string;
  employeeName: string;
  providerName: string;
  supportCategory: string;
  startDate: string;
  endDate: string | null;
  employeePayRate: number | null;
  agreedHourlyRate: number | null;
  isActive: boolean;
}

export interface CreateParticipantServiceAssignmentRequest {
  serviceProviderEmployeeId: string;
  supportCategory: string;
  startDate: string;
  endDate: string | null;
  agreedHourlyRate: number | null;
}

export interface ServiceDelivery {
  id: string;
  participantId: string;
  serviceProviderEmployeeId: string;
  participantServiceAssignmentId: string | null;
  employeeName: string;
  providerName: string;
  supportCategory: string;
  serviceStartUtc: string;
  serviceEndUtc: string;
  serviceHours: number;
  hourlyRate: number;
  amount: number;
  serviceLocation: string | null;
  notes: string | null;
  isClaimed: boolean;
}

export interface CreateServiceDeliveryRequest {
  participantServiceAssignmentId: string;
  serviceStartUtc: string;
  serviceEndUtc: string;
  serviceLocation: string | null;
  notes: string | null;
}
