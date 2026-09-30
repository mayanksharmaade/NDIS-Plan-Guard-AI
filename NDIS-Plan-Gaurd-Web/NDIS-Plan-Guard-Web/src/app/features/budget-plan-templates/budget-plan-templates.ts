import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../core/services/api-error';
import { BudgetPlanTemplate } from './budget-plan-template.models';
import { BudgetPlanTemplateService } from './budget-plan-template.service';

@Component({
  selector: 'app-budget-plan-templates',
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './budget-plan-templates.html',
  styleUrl: './budget-plan-templates.scss'
})
export class BudgetPlanTemplatesComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(BudgetPlanTemplateService);

  readonly templates = signal<BudgetPlanTemplate[]>([]);
  readonly editingId = signal<string | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  readonly form = this.fb.nonNullable.group({
    planName: ['', [Validators.required, Validators.maxLength(150)]],
    budgetType: ['Fortnightly' as 'TotalPlan' | 'Fortnightly', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    isActive: [true]
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.service.getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (templates) => this.templates.set(templates),
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }

  edit(template: BudgetPlanTemplate): void {
    this.editingId.set(template.id);
    this.form.setValue({
      planName: template.planName,
      budgetType: template.budgetType === 'TotalPlan' ? 'TotalPlan' : 'Fortnightly',
      amount: template.amount,
      isActive: template.isActive
    });
  }

  reset(): void {
    this.editingId.set(null);
    this.form.reset({ planName: '', budgetType: 'Fortnightly', amount: 0, isActive: true });
  }

  save(): void {
    this.error.set('');
    this.message.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('Complete the plan template fields.');
      return;
    }

    const raw = this.form.getRawValue();
    const request = {
      planName: raw.planName.trim(),
      budgetType: raw.budgetType,
      amount: Number(raw.amount),
      isActive: raw.isActive
    };

    const id = this.editingId();
    const request$ = id ? this.service.update(id, request) : this.service.create(request);
    this.saving.set(true);
    request$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.message.set(id ? 'Plan template updated.' : 'Plan template created.');
        this.reset();
        this.load();
      },
      error: (e: unknown) => this.error.set(apiErrorMessage(e))
    });
  }
}
