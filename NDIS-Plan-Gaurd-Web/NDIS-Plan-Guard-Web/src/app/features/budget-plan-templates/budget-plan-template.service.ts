import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BudgetPlanTemplate, SaveBudgetPlanTemplateRequest } from './budget-plan-template.models';

@Injectable({ providedIn: 'root' })
export class BudgetPlanTemplateService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/budget-plan-templates`;

  getAll(): Observable<BudgetPlanTemplate[]> {
    return this.http.get<BudgetPlanTemplate[]>(this.url);
  }

  create(request: SaveBudgetPlanTemplateRequest): Observable<BudgetPlanTemplate> {
    return this.http.post<BudgetPlanTemplate>(this.url, request);
  }

  update(id: string, request: SaveBudgetPlanTemplateRequest): Observable<BudgetPlanTemplate> {
    return this.http.put<BudgetPlanTemplate>(`${this.url}/${id}`, request);
  }
}
