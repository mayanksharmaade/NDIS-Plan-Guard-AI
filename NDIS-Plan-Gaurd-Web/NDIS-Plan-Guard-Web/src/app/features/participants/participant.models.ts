export interface CreateParticipantRequest {

  ndisNumber: string;

  firstName: string;

  lastName: string;

  dateOfBirth: string | null;

  email: string | null;

  phoneNumber: string | null;

  planStartDate: string | null;

  planEndDate: string | null;

  emergencyContactName: string | null;

  emergencyContactRelationship: string | null;

  emergencyContactPhoneNumber: string | null;

  planTotalBudget: number | null;
}


export interface UpdateParticipantRequest {

  firstName: string;

  lastName: string;

  dateOfBirth: string | null;

  email: string | null;

  phoneNumber: string | null;

  planStartDate: string | null;

  planEndDate: string | null;

  emergencyContactName: string | null;

  emergencyContactRelationship: string | null;

  emergencyContactPhoneNumber: string | null;

  planTotalBudget: number | null;
}


export interface ParticipantResponse {

  id: string;

  ndisNumber: string;

  firstName: string;

  lastName: string;

  dateOfBirth: string | null;

  email: string | null;

  phoneNumber: string | null;

  planStartDate: string | null;

  planEndDate: string | null;

  emergencyContactName: string | null;

  emergencyContactRelationship:
    string | null;

  emergencyContactPhoneNumber:
    string | null;

  planTotalBudget:
    number | null;

  status: string;

  createdAtUtc: string;
}


export interface CreateParticipantResponse {

  participantId: string;
}