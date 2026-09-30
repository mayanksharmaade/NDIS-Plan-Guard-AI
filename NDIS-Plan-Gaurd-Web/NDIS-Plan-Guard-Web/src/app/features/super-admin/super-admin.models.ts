import { AppRole } from '../../core/auth/auth.models';

export interface CreateAdminRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
}
export interface CreateAdminResponse { userProfileId: string; message: string; }
export interface UserSummaryResponse {
  userProfileId: string;
  identityUserId: string;
  email: string;
  firstName: string;
  lastName: string;
  userCategory: string;
  approvalStatus: string;
  accountStatus: string;
  roles: AppRole[];
}
