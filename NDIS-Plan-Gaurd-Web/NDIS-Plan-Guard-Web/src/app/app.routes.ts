import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppShellComponent } from './shared/components/app-shell/app-shell';

const claimReaders = ['ServiceProvider', 'Admin', 'Reviewer', 'SuperAdmin'] as const;
const reviewers = ['Reviewer', 'Admin', 'SuperAdmin'] as const;

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.RegisterComponent)
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        canActivate: [roleGuard(claimReaders)],
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.DashboardComponent)
      },
      {
        path: 'super-admin',
        canActivate: [roleGuard(['SuperAdmin'])],
        loadComponent: () => import('./features/super-admin/super-admin').then((m) => m.SuperAdminComponent)
      },
      {
        path: 'admin',
        canActivate: [roleGuard(['Admin', 'SuperAdmin'])],
        loadComponent: () => import('./features/admin/admin').then((m) => m.AdminComponent)
      },
      {
        path: 'budget-plan-templates',
        canActivate: [roleGuard(['Admin', 'SuperAdmin'])],
        loadComponent: () => import('./features/budget-plan-templates/budget-plan-templates').then((m) => m.BudgetPlanTemplatesComponent)
      },
      {
        path: 'service-provider',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () => import('./features/service-providers/provider-profile').then((m) => m.ProviderProfileComponent)
      },
      {
        path: 'service-providers/:serviceProviderId/employees',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () =>
          import('./features/provider-employees/provider-employee-list/provider-employee-list.component')
            .then((m) => m.ProviderEmployeeListComponent)
      },
      {
        path: 'participants',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () => import('./features/participants/participants').then((m) => m.ParticipantsComponent)
      },
      {
        path: 'participants/:participantId/budget-plans',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () =>
          import('./features/budget-plans/budget-plan-list/budget-plan-list.component')
            .then((m) => m.BudgetPlanListComponent)
      },
      {
        path: 'participants/:participantId/services',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () =>
          import('./features/participant-services/participant-service-list/participant-service-list.component')
            .then((m) => m.ParticipantServiceListComponent)
      },
      {
        path: 'claims/new',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () => import('./features/claims/editor/claim-editor').then((m) => m.ClaimEditorComponent)
      },
      {
        path: 'claims/:id/edit',
        canActivate: [roleGuard(['ServiceProvider'])],
        loadComponent: () => import('./features/claims/editor/claim-editor').then((m) => m.ClaimEditorComponent)
      },
      {
        path: 'claims',
        canActivate: [roleGuard(claimReaders)],
        loadComponent: () => import('./features/claims/list/claims-list').then((m) => m.ClaimsListComponent)
      },
      {
        path: 'claims/:id',
        canActivate: [roleGuard(claimReaders)],
        loadComponent: () => import('./features/claims/detail/claim-detail').then((m) => m.ClaimDetailComponent)
      },
      {
        path: 'reviews',
        canActivate: [roleGuard(reviewers)],
        loadComponent: () => import('./features/reviews/queue/review-queue').then((m) => m.ReviewQueueComponent)
      },
      {
        path: 'reviews/:id',
        canActivate: [roleGuard(reviewers)],
        loadComponent: () => import('./features/reviews/detail/review-detail').then((m) => m.ReviewDetailComponent)
      },
      {
        path: 'audit',
        canActivate: [roleGuard(claimReaders)],
        loadComponent: () => import('./features/audit/audit').then((m) => m.AuditComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
