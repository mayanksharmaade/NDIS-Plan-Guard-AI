export interface ServiceProviderEmployee {
  id: string;
  serviceProviderId: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  role: string;
  qualifications: string | null;
  defaultHourlyRate: number | null;
  isActive: boolean;
}

export interface SaveServiceProviderEmployeeRequest {
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  role: string;
  qualifications: string | null;
  defaultHourlyRate: number | null;
}
