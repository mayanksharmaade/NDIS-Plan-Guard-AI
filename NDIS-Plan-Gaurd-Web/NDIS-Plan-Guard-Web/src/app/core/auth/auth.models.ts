export type AppRole = 'SuperAdmin' | 'Admin' | 'Reviewer' | 'ServiceProvider';

export interface RegisterServiceProviderRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  legalName: string;
  tradingName: string | null;
  abn: string;
  ndisRegistrationNumber: string | null;
  phoneNumber: string | null;
}

export interface RegisterServiceProviderResponse {
  userProfileId: string;
  serviceProviderId: string;
  email: string;
  approvalStatus: string;
  message: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  userProfileId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: AppRole[];
}

export interface AuthSession extends LoginResponse {}
