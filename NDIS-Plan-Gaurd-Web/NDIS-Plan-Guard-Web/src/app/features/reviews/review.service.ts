import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import {
  ClaimForReviewResponse,
  RecordClaimDecisionRequest,
  ReviewQueueItemResponse
} from './review.models';

import {
  ClaimDecisionSupport
} from './models/claim-decision-support.model';


@Injectable({
  providedIn: 'root'
})
export class ReviewService {

  private readonly http =
    inject(HttpClient);

  private readonly url =
    `${environment.apiBaseUrl}/reviews`;


  queue(): Observable<ReviewQueueItemResponse[]> {

    return this.http.get<ReviewQueueItemResponse[]>(
      `${this.url}/queue`
    );
  }


  getClaim(
    id: string
  ): Observable<ClaimForReviewResponse> {

    return this.http.get<ClaimForReviewResponse>(
      `${this.url}/claims/${id}`
    );
  }


  startReview(
    id: string
  ): Observable<void> {

    return this.http.post<void>(
      `${this.url}/claims/${id}/start`,
      {}
    );
  }


  recordDecision(
    id: string,
    request: RecordClaimDecisionRequest
  ): Observable<void> {

    return this.http.post<void>(
      `${this.url}/claims/${id}/decision`,
      request
    );
  }


  getDecisionSupport(
    claimId: string
  ): Observable<ClaimDecisionSupport> {

    return this.http.get<ClaimDecisionSupport>(
      `${this.url}/${claimId}/decision-support`
    );
  }
}