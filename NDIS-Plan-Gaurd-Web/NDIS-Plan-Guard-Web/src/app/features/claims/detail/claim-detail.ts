import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { apiErrorMessage } from '../../../core/services/api-error';
import { ClaimDetailsResponse } from '../claim.models';
import { ClaimService } from '../claim.service';
import { supportServiceName } from '../../../shared/constants/support-services';

@Component({
  selector: 'app-claim-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './claim-detail.html',
  styleUrl: './claim-detail.scss'
})
export class ClaimDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(ClaimService);
  readonly auth = inject(AuthService);

  readonly claim = signal<ClaimDetailsResponse | null>(null);
  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  readonly supportServiceName = supportServiceName;

  readonly canEdit = computed(() => {
    const claim = this.claim();
    return !!claim &&
      this.auth.hasAnyRole(['ServiceProvider']) &&
      (claim.status === 'Draft' || claim.status === 'MoreInformationRequired');
  });

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.load(id);
    else this.error.set('Claim id is missing.');
  }

  submit(): void {
    const claim = this.claim();
    if (!claim || !this.canEdit()) return;

    this.submitting.set(true);
    this.error.set('');
    this.message.set('');

    this.service
      .submit(claim.id)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          this.message.set(response.message);
          this.load(claim.id);
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  private load(id: string): void {
    this.loading.set(true);
    this.error.set('');

    this.service
      .get(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (claim) => this.claim.set(claim),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }
}
