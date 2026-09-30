import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';


import {
  ClaimDecisionSupport
} from '../models/claim-decision-support.model';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly http = inject(HttpClient);

  private readonly apiUrl = environment.apiBaseUrl;

  getDecisionSupport(
    claimId: string
  ): Observable<ClaimDecisionSupport> {

    return this.http.get<ClaimDecisionSupport>(
      `${this.apiUrl}/api/reviews/${claimId}/decision-support`
    );
  }
}