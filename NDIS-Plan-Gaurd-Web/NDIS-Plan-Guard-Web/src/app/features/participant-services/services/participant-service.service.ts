import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateParticipantServiceAssignmentRequest,
  CreateServiceDeliveryRequest,
  ParticipantServiceAssignment,
  ServiceDelivery
} from '../models/participant-service.model';

@Injectable({ providedIn: 'root' })
export class ParticipantServiceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiBaseUrl;

  getAssignments(participantId: string): Observable<ParticipantServiceAssignment[]> {
    return this.http.get<ParticipantServiceAssignment[]>(
      `${this.apiUrl}/participants/${participantId}/service-assignments`
    );
  }

  assignEmployee(
    participantId: string,
    request: CreateParticipantServiceAssignmentRequest
  ): Observable<ParticipantServiceAssignment> {
    return this.http.post<ParticipantServiceAssignment>(
      `${this.apiUrl}/participants/${participantId}/service-assignments`,
      request
    );
  }

  getDeliveries(participantId: string): Observable<ServiceDelivery[]> {
    return this.http.get<ServiceDelivery[]>(
      `${this.apiUrl}/participants/${participantId}/service-deliveries`
    );
  }

  recordDelivery(
    participantId: string,
    request: CreateServiceDeliveryRequest
  ): Observable<ServiceDelivery> {
    return this.http.post<ServiceDelivery>(
      `${this.apiUrl}/participants/${participantId}/service-deliveries`,
      request
    );
  }
}
