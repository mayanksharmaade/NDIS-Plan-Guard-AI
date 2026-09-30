import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { apiErrorMessage } from '../../../core/services/api-error';
import { BudgetPlanTemplate } from '../../budget-plan-templates/budget-plan-template.models';
import { BudgetPlanTemplateService } from '../../budget-plan-templates/budget-plan-template.service';
import { ParticipantResponse } from '../../participants/participant.models';
import { ParticipantService } from '../../participants/participant.service';
import { ParticipantBudgetPlan, SaveParticipantBudgetPlanRequest } from '../models/participant-budget-plan.model';
import { ParticipantBudgetPlanService } from '../services/participant-budget-plan.service';

@Component({
  selector: 'app-budget-plan-list',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './budget-plan-list.component.html',
  styleUrl: './budget-plan-list.component.scss'
})
export class BudgetPlanListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly participantService = inject(ParticipantService);
  private readonly templateService = inject(BudgetPlanTemplateService);
  private readonly service = inject(ParticipantBudgetPlanService);

  readonly participantId = this.route.snapshot.paramMap.get('participantId') ?? '';
  readonly participant = signal<ParticipantResponse | null>(null);
  readonly templates = signal<BudgetPlanTemplate[]>([]);
  readonly plans = signal<ParticipantBudgetPlan[]>([]);
  readonly loading = signal(false);
  readonly savingPlan = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  readonly selectedTemplate = computed(() => {
    const id = this.planForm.controls.budgetPlanTemplateId.value;
    return this.templates().find((item) => item.id === id) ?? null;
  });

  readonly planForm = this.fb.nonNullable.group({
    budgetPlanTemplateId: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    status: ['Active', Validators.required]
  });

  constructor() {
    this.load();
  }

  load(): void {
    if (!this.participantId) {
      this.error.set('Participant id is missing from the route.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    forkJoin({
      participant: this.participantService.get(this.participantId),
      templates: this.templateService.getAll(),
      plans: this.service.getAll(this.participantId)
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ({ participant, templates, plans }) => {
          this.participant.set(participant);
          this.templates.set(templates.filter((item) => item.isActive));
          this.plans.set(plans);
        },
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }

  createPlan(): void {
    this.error.set('');
    this.message.set('');
    if (this.planForm.invalid) {
      this.planForm.markAllAsTouched();
      this.error.set('Select a plan template and enter the plan dates.');
      return;
    }

    const raw = this.planForm.getRawValue();
    if (raw.endDate < raw.startDate) {
      this.error.set('Plan end date cannot be before the start date.');
      return;
    }

    const request: SaveParticipantBudgetPlanRequest = {
      budgetPlanTemplateId: raw.budgetPlanTemplateId,
      startDate: raw.startDate,
      endDate: raw.endDate,
      status: raw.status
    };

    this.savingPlan.set(true);
    this.service.create(this.participantId, request)
      .pipe(finalize(() => this.savingPlan.set(false)))
      .subscribe({
        next: () => {
          this.message.set('Participant budget plan created from the selected template.');
          this.planForm.reset({ budgetPlanTemplateId: '', startDate: '', endDate: '', status: 'Active' });
          this.load();
        },
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }
}
