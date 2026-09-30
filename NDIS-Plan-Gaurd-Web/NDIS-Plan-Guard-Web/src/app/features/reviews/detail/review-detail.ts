import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';

import { finalize } from 'rxjs';

import {
  apiErrorMessage
} from '../../../core/services/api-error';

import {
  ClaimForReviewResponse,
  ClaimReviewDecision
} from '../review.models';

import {
  ClaimDecisionSupport
} from '../models/claim-decision-support.model';

import {
  ReviewService
} from '../review.service';


@Component({
  selector: 'app-review-detail',

  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe
  ],

  templateUrl: './review-detail.html',
  styleUrl: './review-detail.scss'
})
export class ReviewDetailComponent {

  private readonly fb =
    inject(FormBuilder);

  private readonly route =
    inject(ActivatedRoute);

  private readonly service =
    inject(ReviewService);


  readonly claim =
    signal<ClaimForReviewResponse | null>(null);


  /*
   * NEW:
   * ML / decision-support data returned from:
   *
   * GET /api/reviews/{claimId}/decision-support
   */
  readonly decisionSupport =
    signal<ClaimDecisionSupport | null>(null);


  readonly loading =
    signal(false);

  readonly starting =
    signal(false);

  readonly deciding =
    signal(false);

  readonly error =
    signal('');

  readonly riskError =
    signal('');

  readonly message =
    signal('');


  readonly claimId =
    this.route.snapshot.paramMap.get('id');


  readonly form =
    this.fb.nonNullable.group({

      decision: [
        'Approved' as ClaimReviewDecision,
        Validators.required
      ],

      comments: [
        '',
        Validators.maxLength(1000)
      ]
    });


  constructor() {

    if (this.claimId) {

      this.load();

    } else {

      this.error.set(
        'Claim id is missing.'
      );
    }
  }


  startReview(): void {

    if (!this.claimId)
      return;


    this.starting.set(true);

    this.error.set('');

    this.message.set('');


    this.service
      .startReview(this.claimId)
      .pipe(
        finalize(
          () => this.starting.set(false)
        )
      )
      .subscribe({

        next: () => {

          this.message.set(
            'Review started.'
          );

          this.load();
        },

        error: (error: unknown) => {

          this.error.set(
            apiErrorMessage(error)
          );
        }
      });
  }


  recordDecision(): void {

    const claim =
      this.claim();


    if (
      !claim
      ||
      claim.status !== 'Processing'
    ) {
      return;
    }


    if (this.form.invalid) {

      this.form.markAllAsTouched();

      return;
    }


    const value =
      this.form.getRawValue();


    const comments =
      value.comments.trim();


    if (
      (
        value.decision === 'Rejected'
        ||
        value.decision ===
          'MoreInformationRequired'
      )
      &&
      !comments
    ) {

      this.error.set(
        'Comments are required when rejecting a claim or requesting more information.'
      );

      return;
    }


    this.deciding.set(true);

    this.error.set('');

    this.message.set('');


    this.service
      .recordDecision(
        claim.claimId,
        {
          decision:
            value.decision,

          comments:
            comments || null
        }
      )
      .pipe(
        finalize(
          () => this.deciding.set(false)
        )
      )
      .subscribe({

        next: () => {

          this.message.set(
            `Decision recorded: ${value.decision}.`
          );


          this.form.reset({
            decision: 'Approved',
            comments: ''
          });


          this.load();
        },

        error: (error: unknown) => {

          this.error.set(
            apiErrorMessage(error)
          );
        }
      });
  }


  private load(): void {

    if (!this.claimId)
      return;


    this.loading.set(true);

    this.error.set('');

    this.riskError.set('');


    /*
     * Existing claim/review data.
     */
    this.service
      .getClaim(this.claimId)
      .pipe(
        finalize(
          () => this.loading.set(false)
        )
      )
      .subscribe({

        next: claim => {

          this.claim.set(claim);
        },

        error: (error: unknown) => {

          this.error.set(
            apiErrorMessage(error)
          );
        }
      });


    /*
     * NEW:
     * Load ML risk + validation findings separately.
     *
     * If this fails, the normal review screen
     * still continues working.
     */
    this.service
      .getDecisionSupport(this.claimId)
      .subscribe({

       next: (result: ClaimDecisionSupport) => {
  this.decisionSupport.set(result);
},

        error: (error: unknown) => {

          console.error(
            'Unable to load claim decision support',
            error
          );


          this.riskError.set(
            'Risk information is currently unavailable.'
          );
        }
      });
  }
}