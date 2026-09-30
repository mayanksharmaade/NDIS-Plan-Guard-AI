import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { apiErrorMessage } from '../../../core/services/api-error';
import { ClaimListItemResponse } from '../claim.models';
import { ClaimService } from '../claim.service';
import { supportServiceName } from '../../../shared/constants/support-services';

@Component({
  selector: 'app-claims-list',
  imports: [RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './claims-list.html',
  styleUrl: './claims-list.scss'
})
export class ClaimsListComponent {
  private readonly service = inject(ClaimService);
  readonly auth = inject(AuthService);

  readonly supportServiceName = supportServiceName;

  readonly claims = signal<ClaimListItemResponse[]>([]);
  readonly search = signal('');
  readonly loading = signal(false);
  readonly error = signal('');

  readonly filtered = computed(() => {
    const query = this.search().trim().toLowerCase();
    if (!query) return this.claims();

    return this.claims().filter((claim) =>
      `${claim.claimNumber} ${claim.participantName} ${claim.supportCategory} ${claim.status}`
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
      .list()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (claims) => this.claims.set(claims),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  canEdit(status: string): boolean {
    return this.auth.hasAnyRole(['ServiceProvider']) &&
      (status === 'Draft' || status === 'MoreInformationRequired');
  }
}
