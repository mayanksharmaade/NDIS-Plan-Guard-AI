import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import {
  ClaimDecisionSupport
} from '../models/claim-decision-support.model';

@Component({
  selector: 'app-decision-support-panel',

  standalone: true,

  imports: [
    CommonModule
  ],

  templateUrl:
    './decision-support-panel.component.html',

  styleUrls: [
    './decision-support-panel.component.scss'
  ]
})
export class DecisionSupportPanelComponent {

  @Input({ required: true })
  model!: ClaimDecisionSupport;

  findingClass(severity: string): string {
    return (
      severity
      ?? 'unknown'
    ).toLowerCase();
  }

  riskClass(): string {
    return (
      this.model?.mlRisk?.riskBand
      ?? 'unknown'
    ).toLowerCase();
  }
}