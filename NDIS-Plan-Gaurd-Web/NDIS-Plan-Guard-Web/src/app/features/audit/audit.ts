import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../core/services/api-error';
import { AuditEventResponse } from './audit.models';
import { AuditService } from './audit.service';

@Component({
  selector: 'app-audit',
  imports: [DatePipe],
  templateUrl: './audit.html',
  styleUrl: './audit.scss'
})
export class AuditComponent {
  private readonly service = inject(AuditService);
  private readonly route = inject(ActivatedRoute);

  readonly events = signal<AuditEventResponse[]>([]);
  readonly search = signal('');
  readonly entityType = signal(this.route.snapshot.queryParamMap.get('entityType') ?? '');
  readonly entityId = signal(this.route.snapshot.queryParamMap.get('entityId') ?? '');
  readonly loading = signal(false);
  readonly error = signal('');

  readonly filtered = computed(() => {
    const query = this.search().trim().toLowerCase();
    if (!query) return this.events();

    return this.events().filter((event) =>
      `${event.action} ${event.description} ${event.actorEmail ?? ''} ${event.entityType} ${event.entityId}`
        .toLowerCase()
        .includes(query)
    );
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');

    this.service
      .getEvents(100, this.entityType(), this.entityId())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (events) => this.events.set(events),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  clearFilters(): void {
    this.entityType.set('');
    this.entityId.set('');
    this.search.set('');
    this.load();
  }
}
