export interface PendingRegistrationResponse {
  userProfileId: string;
  serviceProviderId: string;
  email: string;
  firstName: string;
  lastName: string;
  legalName: string;
  tradingName: string | null;
  abn: string;
  ndisRegistrationNumber: string | null;
  registeredAtUtc: string;
}
