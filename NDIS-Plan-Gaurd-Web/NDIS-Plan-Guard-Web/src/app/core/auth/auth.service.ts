import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AppRole,
  AuthSession,
  LoginRequest,
  LoginResponse,
  RegisterServiceProviderRequest,
  RegisterServiceProviderResponse
} from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storageKey = 'ndis-plan-guard.auth';

  private readonly sessionSignal = signal<AuthSession | null>(this.readSession());

  readonly session = this.sessionSignal.asReadonly();
  readonly isAuthenticated = computed(() => {
    const session = this.sessionSignal();
    return !!session && new Date(session.expiresAtUtc).getTime() > Date.now();
  });
  readonly roles = computed(() => this.sessionSignal()?.roles ?? []);
  readonly displayName = computed(() => {
    const session = this.sessionSignal();
    return session ? `${session.firstName} ${session.lastName}`.trim() : '';
  });

  registerServiceProvider(
    request: RegisterServiceProviderRequest
  ): Observable<RegisterServiceProviderResponse> {
    return this.http.post<RegisterServiceProviderResponse>(
      `${environment.apiBaseUrl}/auth/register/service-provider`,
      request
    );
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.sessionSignal.set(null);
    void this.router.navigateByUrl('/login');
  }

  accessToken(): string | null {
    return this.isAuthenticated() ? this.sessionSignal()?.accessToken ?? null : null;
  }

  hasAnyRole(roles: readonly AppRole[]): boolean {
    return this.roles().some((role) => roles.includes(role));
  }

  defaultRoute(): string {
    return '/dashboard';
  }

  private setSession(response: LoginResponse): void {
    localStorage.setItem(this.storageKey, JSON.stringify(response));
    this.sessionSignal.set(response);
  }

  private readSession(): AuthSession | null {
    const raw = localStorage.getItem(this.storageKey);
    if (!raw) return null;

    try {
      const session = JSON.parse(raw) as AuthSession;
      if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
        localStorage.removeItem(this.storageKey);
        return null;
      }
      return session;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
