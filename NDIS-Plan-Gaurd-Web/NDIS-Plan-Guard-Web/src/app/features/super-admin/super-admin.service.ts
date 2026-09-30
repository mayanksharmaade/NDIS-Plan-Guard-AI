import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateAdminRequest, CreateAdminResponse, UserSummaryResponse } from './super-admin.models';

@Injectable({ providedIn: 'root' })
export class SuperAdminService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/super-admin`;

  getUsers(): Observable<UserSummaryResponse[]> { return this.http.get<UserSummaryResponse[]>(`${this.url}/users`); }
  createAdmin(request: CreateAdminRequest): Observable<CreateAdminResponse> { return this.http.post<CreateAdminResponse>(`${this.url}/admins`, request); }
  activateUser(userProfileId: string): Observable<void> { return this.http.post<void>(`${this.url}/users/${userProfileId}/activate`, {}); }
  disableUser(userProfileId: string): Observable<void> { return this.http.post<void>(`${this.url}/users/${userProfileId}/disable`, {}); }
}
