import { HttpErrorResponse } from '@angular/common/http';

interface ErrorPayload {
  error?: string;
  detail?: string;
  message?: string;
  errors?: string[] | Record<string, string[]>;
}

export function apiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Unexpected error. Please try again.';

  const payload = error.error as ErrorPayload | string | null;
  if (typeof payload === 'string' && payload.trim()) return payload;
  if (!payload || typeof payload !== 'object') return `Request failed (${error.status}).`;

  if (Array.isArray(payload.errors) && payload.errors.length) return payload.errors.join(' ');
  if (payload.errors && !Array.isArray(payload.errors)) {
    const messages = Object.values(payload.errors).flat();
    if (messages.length) return messages.join(' ');
  }

  return payload.error ?? payload.detail ?? payload.message ?? `Request failed (${error.status}).`;
}
