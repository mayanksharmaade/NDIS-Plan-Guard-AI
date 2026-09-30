import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ParticipantBudgetPlan, SaveParticipantBudgetPlanRequest } from '../models/participant-budget-plan.model';

@Injectable({ providedIn: 'root' })
export class ParticipantBudgetPlanService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiBaseUrl;

  getAll(participantId: string): Observable<ParticipantBudgetPlan[]> {
    return this.http.get<ParticipantBudgetPlan[]>(`${this.apiUrl}/participants/${participantId}/budget-plans`);
  }

  create(participantId: string, request: SaveParticipantBudgetPlanRequest): Observable<ParticipantBudgetPlan> {
    return this.http.post<ParticipantBudgetPlan>(`${this.apiUrl}/participants/${participantId}/budget-plans`, request);
  }
}
