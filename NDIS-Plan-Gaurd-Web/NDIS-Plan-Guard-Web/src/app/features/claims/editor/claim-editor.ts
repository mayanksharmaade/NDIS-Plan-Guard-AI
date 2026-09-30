import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { apiErrorMessage } from '../../../core/services/api-error';
import { SUPPORT_SERVICES, supportServiceName } from '../../../shared/constants/support-services';
import { ParticipantResponse } from '../../participants/participant.models';
import { ParticipantService } from '../../participants/participant.service';
import { ClaimDetailsResponse, EligibleServiceDeliveryResponse } from '../claim.models';
import { ClaimService } from '../claim.service';

@Component({
  selector: 'app-claim-editor',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './claim-editor.html',
  styleUrl: './claim-editor.scss'
})
export class ClaimEditorComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly claimService = inject(ClaimService);
  private readonly participantService = inject(ParticipantService);

  readonly participants = signal<ParticipantResponse[]>([]);
  readonly claim = signal<ClaimDetailsResponse | null>(null);
  readonly eligibleDeliveries = signal<EligibleServiceDeliveryResponse[]>([]);
  readonly selectedDeliveryIds = signal<string[]>([]);
  readonly loading = signal(false);
  readonly loadingDeliveries = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  readonly editingId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.editingId;
  readonly supportServices = SUPPORT_SERVICES;
  readonly supportName = supportServiceName;

  readonly form = this.fb.nonNullable.group({
    participantId: ['', Validators.required],
    serviceFrom: ['', Validators.required],
    serviceTo: ['', Validators.required],
    supportCategory: ['', Validators.required],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
    amount: [0]
  });

  readonly filteredDeliveries = computed(() => {
    const code = this.form.controls.supportCategory.value;
    if (!code) {
      return this.eligibleDeliveries();
    }
    return this.eligibleDeliveries().filter((item) => item.supportCategory === code);
  });

  readonly selectedDeliveries = computed(() => {
    const selected = new Set(this.selectedDeliveryIds());
    return this.eligibleDeliveries().filter((item) => selected.has(item.id));
  });

  readonly totalHours = computed(() =>
    Math.round(this.selectedDeliveries().reduce((sum, item) => sum + item.serviceHours, 0) * 100) / 100
  );

  readonly totalAmount = computed(() =>
    Math.round(this.selectedDeliveries().reduce((sum, item) => sum + item.amount, 0) * 100) / 100
  );

  readonly selectedEmployeeName = computed(() => this.selectedDeliveries()[0]?.employeeName ?? '—');
  readonly selectedBillingRate = computed(() => this.selectedDeliveries()[0]?.agreedHourlyRate ?? null);

  constructor() {
    this.load();
  }

  loadEligibleDeliveries(): void {
    this.error.set('');
    this.message.set('');
    this.selectedDeliveryIds.set([]);
    this.eligibleDeliveries.set([]);

    const value = this.form.getRawValue();
    if (!value.participantId || !value.serviceFrom || !value.serviceTo) {
      this.error.set('Select a participant and claim period first.');
      return;
    }

    if (value.serviceTo < value.serviceFrom) {
      this.error.set('Service-to date cannot be before service-from date.');
      return;
    }

    this.loadingDeliveries.set(true);
    this.claimService
      .eligibleDeliveries(value.participantId, value.serviceFrom, value.serviceTo)
      .pipe(finalize(() => this.loadingDeliveries.set(false)))
      .subscribe({
        next: (rows) => {
          this.eligibleDeliveries.set(rows);
          if (rows.length === 0) {
            this.message.set('No unclaimed recorded service deliveries were found for this period.');
          }
        },
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }

  toggleDelivery(delivery: EligibleServiceDeliveryResponse, checked: boolean): void {
    const ids = new Set(this.selectedDeliveryIds());

    if (!checked) {
      ids.delete(delivery.id);
      this.selectedDeliveryIds.set([...ids]);
      return;
    }

    const current = this.selectedDeliveries();
    if (current.length > 0) {
      const first = current[0];
      if (first.serviceProviderEmployeeId !== delivery.serviceProviderEmployeeId) {
        this.error.set('Create a separate claim for a different support worker.');
        return;
      }
      if (first.supportCategory !== delivery.supportCategory) {
        this.error.set('Create a separate claim for a different support service.');
        return;
      }
      if (first.agreedHourlyRate !== delivery.agreedHourlyRate) {
        this.error.set('Create a separate claim when the agreed billing rate is different.');
        return;
      }
    }

    this.error.set('');
    ids.add(delivery.id);
    this.selectedDeliveryIds.set([...ids]);
  }

  isSelected(id: string): boolean {
    return this.selectedDeliveryIds().includes(id);
  }

  selectAllVisible(): void {
    const rows = this.filteredDeliveries();
    if (rows.length === 0) {
      return;
    }

    const first = rows[0];
    const compatible = rows.filter((item) =>
      item.serviceProviderEmployeeId === first.serviceProviderEmployeeId
      && item.supportCategory === first.supportCategory
      && item.agreedHourlyRate === first.agreedHourlyRate
    );
    this.selectedDeliveryIds.set(compatible.map((item) => item.id));
  }

  save(): void {
    this.error.set('');
    this.message.set('');

    if (this.isEdit) {
      this.saveLegacyEdit();
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('Complete the participant, period, service and description.');
      return;
    }

    const deliveries = this.selectedDeliveries();
    if (deliveries.length === 0) {
      this.error.set('Select at least one recorded service delivery.');
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.claimService.create({
      participantId: value.participantId,
      serviceFrom: value.serviceFrom,
      serviceTo: value.serviceTo,
      supportCategory: deliveries[0].supportCategory,
      description: value.description.trim(),
      amount: this.totalAmount(),
      units: null,
      unitPrice: null,
      serviceDeliveryIds: deliveries.map((item) => item.id)
    })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (created) => void this.router.navigate(['/claims', created.claimId]),
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }

  private saveLegacyEdit(): void {
    const claim = this.claim();
    if (!claim || !this.editingId) {
      return;
    }

    if (claim.serviceDeliveries?.length) {
      this.error.set('Delivery-based claims are calculated from recorded service deliveries and cannot be manually edited.');
      return;
    }

    const value = this.form.getRawValue();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.claimService.update(this.editingId, {
      serviceFrom: value.serviceFrom,
      serviceTo: value.serviceTo,
      supportCategory: value.supportCategory,
      description: value.description.trim(),
      amount: Number(value.amount)
    })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => void this.router.navigate(['/claims', this.editingId]),
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set('');

    if (!this.editingId) {
      this.participantService.list()
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe({
          next: (participants) => this.participants.set(participants.filter((p) => p.status === 'Active')),
          error: (e: unknown) => this.error.set(apiErrorMessage(e))
        });
      return;
    }

    forkJoin({
      participants: this.participantService.list(),
      claim: this.claimService.get(this.editingId)
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ({ participants, claim }) => {
          this.participants.set(participants.filter((p) => p.status === 'Active'));
          this.claim.set(claim);
          this.form.setValue({
            participantId: claim.participantId,
            serviceFrom: this.dateInput(claim.serviceFrom),
            serviceTo: this.dateInput(claim.serviceTo),
            supportCategory: claim.supportCategory,
            description: claim.description,
            amount: claim.amount
          });
          this.form.controls.participantId.disable();

          if (claim.serviceDeliveries?.length) {
            this.form.disable();
            this.message.set('This claim is linked to recorded service deliveries. Service hours, rate and amount are read-only.');
          } else if (claim.status !== 'Draft' && claim.status !== 'MoreInformationRequired') {
            this.form.disable();
            this.error.set('Only draft claims or claims awaiting more information can be edited.');
          }
        },
        error: (e: unknown) => this.error.set(apiErrorMessage(e))
      });
  }

  private dateInput(value: string): string {
    return value.substring(0, 10);
  }
}
