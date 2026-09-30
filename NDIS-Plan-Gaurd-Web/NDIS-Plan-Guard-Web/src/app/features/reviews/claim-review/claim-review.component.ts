import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import {
  ClaimDecisionSupport
} from '../models/claim-decision-support.model';

import {
  ReviewService
} from '../services/review.service';



@Component({
  selector: 'app-claim-review',

  standalone: true,

  imports: [
    CommonModule
    
  ],

  templateUrl:
    './claim-review.component.html',

  styleUrls: [
    './claim-review.component.scss'
  ]
})
export class ClaimReviewComponent
  implements OnInit {

  model?: ClaimDecisionSupport;

  loading = true;

  error?: string;

  constructor(
    private readonly route:
      ActivatedRoute,

    private readonly reviewService:
      ReviewService
  ) {}

  ngOnInit(): void {

    const claimId =
      this.route.snapshot.paramMap.get('id');

    if (!claimId) {

      this.error =
        'Claim identifier was not supplied';

      this.loading = false;

      return;
    }

    this.reviewService
      .getDecisionSupport(claimId)
      .subscribe({

        next: result => {

          this.model = result;

          this.loading = false;
        },

        error: error => {

          console.error(
            'Unable to load claim decision support',
            error
          );

          this.error =
            'Unable to load claim decision support';

          this.loading = false;
        }
      });
  }
}