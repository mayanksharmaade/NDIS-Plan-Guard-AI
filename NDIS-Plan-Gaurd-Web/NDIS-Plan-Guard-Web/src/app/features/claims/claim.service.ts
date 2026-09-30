import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ClaimDetailsResponse,
  ClaimListItemResponse,
  ClaimMutationResponse,
  CreateClaimRequest,
  EligibleServiceDeliveryResponse,
  UpdateClaimRequest
} from './claim.models';

@Injectable({ providedIn: 'root' })
export class ClaimService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/claims`;

  list(): Observable<ClaimListItemResponse[]> {
    return this.http.get<ClaimListItemResponse[]>(this.url);
  }

  get(id: string): Observable<ClaimDetailsResponse> {
    return this.http.get<ClaimDetailsResponse>(`${this.url}/${id}`);
  }

  eligibleDeliveries(
    participantId: string,
    serviceFrom: string,
    serviceTo: string
  ): Observable<EligibleServiceDeliveryResponse[]> {
    const params = new HttpParams()
      .set('participantId', participantId)
      .set('serviceFrom', serviceFrom)
      .set('serviceTo', serviceTo);

    return this.http.get<EligibleServiceDeliveryResponse[]>(`${this.url}/eligible-deliveries`, { params });
  }

  create(request: CreateClaimRequest): Observable<ClaimMutationResponse> {
    return this.http.post<ClaimMutationResponse>(this.url, request);
  }

  update(id: string, request: UpdateClaimRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  submit(id: string): Observable<ClaimMutationResponse> {
    return this.http.post<ClaimMutationResponse>(`${this.url}/${id}/submit`, {});
  }
}
