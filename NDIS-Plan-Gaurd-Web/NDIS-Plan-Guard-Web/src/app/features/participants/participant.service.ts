import {
  inject,
  Injectable
} from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

import {
  Observable
} from 'rxjs';

import {
  environment
} from '../../../environments/environment';

import {
  CreateParticipantRequest,
  CreateParticipantResponse,
  ParticipantResponse,
  UpdateParticipantRequest
} from './participant.models';


@Injectable({
  providedIn: 'root'
})
export class ParticipantService {

  private readonly http =
    inject(HttpClient);

  private readonly url =
    `${environment.apiBaseUrl}/participants`;


  list():
    Observable<ParticipantResponse[]> {

    return this.http
      .get<ParticipantResponse[]>(
        this.url
      );
  }


  get(
    id: string
  ): Observable<ParticipantResponse> {

    return this.http
      .get<ParticipantResponse>(
        `${this.url}/${id}`
      );
  }


  create(
    request: CreateParticipantRequest
  ): Observable<CreateParticipantResponse> {

    return this.http
      .post<CreateParticipantResponse>(
        this.url,
        request
      );
  }


  update(
    id: string,
    request: UpdateParticipantRequest
  ): Observable<void> {

    return this.http
      .put<void>(
        `${this.url}/${id}`,
        request
      );
  }


  deactivate(
    id: string
  ): Observable<void> {

    return this.http
      .post<void>(
        `${this.url}/${id}/deactivate`,
        {}
      );
  }
}