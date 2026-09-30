import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AppRole } from '../auth/auth.models';
import { AuthService } from '../auth/auth.service';

export const roleGuard = (roles: readonly AppRole[]): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) return router.createUrlTree(['/login']);
  return auth.hasAnyRole(roles) ? true : router.createUrlTree(['/dashboard']);
};
