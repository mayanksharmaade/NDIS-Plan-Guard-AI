import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../../core/services/api-error';
import { ReviewQueueItemResponse } from '../review.models';
import { ReviewService } from '../review.service';

@Component({
  selector: 'app-review-queue',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './review-queue.html',
  styleUrl: './review-queue.scss'
})
export class ReviewQueueComponent {
  private readonly service = inject(ReviewService);
  private readonly router = inject(Router);

  readonly claims = signal<ReviewQueueItemResponse[]>([]);
  readonly search = signal('');
  readonly loading = signal(false);
  readonly startingId = signal<string | null>(null);
  readonly error = signal('');

  readonly filtered = computed(() => {
    const query = this.search().trim().toLowerCase();
    if (!query) return this.claims();

    return this.claims().filter((claim) =>
      `${claim.claimNumber} ${claim.serviceProviderName} ${claim.participantName} ${claim.participantNdisNumber} ${claim.status}`
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
      .queue()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (claims) => this.claims.set(claims),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  open(claim: ReviewQueueItemResponse): void {
    if (claim.status === 'Processing') {
      void this.router.navigate(['/reviews', claim.claimId]);
      return;
    }

    this.startingId.set(claim.claimId);
    this.error.set('');

    this.service
      .startReview(claim.claimId)
      .pipe(finalize(() => this.startingId.set(null)))
      .subscribe({
        next: () => void this.router.navigate(['/reviews', claim.claimId]),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }
}
