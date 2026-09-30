import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  SaveServiceProviderEmployeeRequest,
  ServiceProviderEmployee
} from '../models/service-provider-employee.model';

@Injectable({ providedIn: 'root' })
export class ProviderEmployeeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiBaseUrl;

  getAll(serviceProviderId: string): Observable<ServiceProviderEmployee[]> {
    return this.http.get<ServiceProviderEmployee[]>(
      `${this.apiUrl}/service-providers/${serviceProviderId}/employees`
    );
  }

  create(
    serviceProviderId: string,
    request: SaveServiceProviderEmployeeRequest
  ): Observable<ServiceProviderEmployee> {
    return this.http.post<ServiceProviderEmployee>(
      `${this.apiUrl}/service-providers/${serviceProviderId}/employees`,
      request
    );
  }

  update(
    serviceProviderId: string,
    employeeId: string,
    request: SaveServiceProviderEmployeeRequest
  ): Observable<ServiceProviderEmployee> {
    return this.http.put<ServiceProviderEmployee>(
      `${this.apiUrl}/service-providers/${serviceProviderId}/employees/${employeeId}`,
      request
    );
  }

  setActive(
    serviceProviderId: string,
    employeeId: string,
    isActive: boolean
  ): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/service-providers/${serviceProviderId}/employees/${employeeId}/active`,
      isActive
    );
  }
}
