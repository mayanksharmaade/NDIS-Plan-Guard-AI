import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceProviderProfileResponse, UpdateServiceProviderProfileRequest } from './provider.models';

@Injectable({ providedIn: 'root' })
export class ProviderService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/service-providers/me`;
  getMine(): Observable<ServiceProviderProfileResponse> { return this.http.get<ServiceProviderProfileResponse>(this.url); }
  updateMine(request: UpdateServiceProviderProfileRequest): Observable<void> { return this.http.put<void>(this.url, request); }
}
