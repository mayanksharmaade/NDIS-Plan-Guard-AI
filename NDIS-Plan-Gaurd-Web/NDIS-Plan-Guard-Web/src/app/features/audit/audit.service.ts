import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditEventResponse } from './audit.models';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/audit/events`;

  getEvents(take = 100, entityType?: string, entityId?: string): Observable<AuditEventResponse[]> {
    let params = new HttpParams().set('take', take);

    if (entityType?.trim()) params = params.set('entityType', entityType.trim());
    if (entityId?.trim()) params = params.set('entityId', entityId.trim());

    return this.http.get<AuditEventResponse[]>(this.url, { params });
  }
}
