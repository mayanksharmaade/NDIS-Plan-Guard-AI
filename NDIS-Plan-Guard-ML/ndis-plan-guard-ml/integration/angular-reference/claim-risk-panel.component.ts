import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ClaimRiskResult } from './claim-risk.model';

@Component({
  selector: 'app-claim-risk-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="risk-panel" *ngIf="result">
      <header>
        <div>
          <p class="eyebrow">ML decision support</p>
          <h3>Claim risk</h3>
        </div>
        <div class="risk-score" [attr.data-band]="result.riskBand.toLowerCase()">
          <strong>{{ result.riskScore }}/100</strong>
          <span>{{ result.riskBand }}</span>
        </div>
      </header>

      <h4>Contributing factors</h4>
      <ul>
        <li *ngFor="let factor of result.topFactors">
          <span>{{ factor.description }}</span>
          <small>impact {{ factor.probabilityImpact | number:'1.3-3' }}</small>
        </li>
      </ul>

      <p class="notice">{{ result.decisionSupportNotice }}</p>
      <small>Model {{ result.modelVersion }} · Features {{ result.featureVersion }}</small>
    </section>
  `,
  styleUrl: './claim-risk-panel.component.scss'
})
export class ClaimRiskPanelComponent {
  @Input({ required: true }) result!: ClaimRiskResult;
}
