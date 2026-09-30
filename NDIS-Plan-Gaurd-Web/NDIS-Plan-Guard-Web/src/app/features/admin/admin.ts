import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../core/services/api-error';
import { AdminService } from './admin.service';
import { PendingRegistrationResponse } from './admin.models';

@Component({ selector: 'app-admin', imports: [DatePipe], templateUrl: './admin.html', styleUrl: './admin.scss' })
export class AdminComponent {
  private readonly service = inject(AdminService);
  readonly rows = signal<PendingRegistrationResponse[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  constructor() { this.load(); }
  load(): void { this.loading.set(true); this.service.pending().pipe(finalize(() => this.loading.set(false))).subscribe({ next: x => this.rows.set(x), error:  (e: unknown)=> this.error.set(apiErrorMessage(e)) }); }
  decide(row: PendingRegistrationResponse, approve: boolean): void {
    const request = approve ? this.service.approve(row.userProfileId) : this.service.reject(row.userProfileId);
    request.subscribe({ next: () => { this.message.set(approve ? 'Registration approved.' : 'Registration rejected.'); this.load(); }, error:  (e: unknown) => this.error.set(apiErrorMessage(e)) });
  }
}
