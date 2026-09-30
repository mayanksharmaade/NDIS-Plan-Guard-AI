export interface ServiceProviderProfileResponse {
  serviceProviderId: string;
  userProfileId: string;
  email: string;
  firstName: string;
  lastName: string;
  legalName: string;
  tradingName: string | null;
  abn: string;
  ndisRegistrationNumber: string | null;
  approvalStatus: string;
  status: string;
}
export interface UpdateServiceProviderProfileRequest {
  firstName: string;
  lastName: string;
  legalName: string;
  tradingName: string | null;
  abn: string;
  ndisRegistrationNumber: string | null;
}
