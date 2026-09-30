import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PendingRegistrationResponse } from './admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/admin/users`;
  pending(): Observable<PendingRegistrationResponse[]> { return this.http.get<PendingRegistrationResponse[]>(`${this.url}/registrations/pending`); }
  approve(id: string): Observable<void> { return this.http.post<void>(`${this.url}/registrations/${id}/approve`, {}); }
  reject(id: string): Observable<void> { return this.http.post<void>(`${this.url}/registrations/${id}/reject`, {}); }
}
