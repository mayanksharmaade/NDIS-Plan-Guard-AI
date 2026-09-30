import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../../core/services/api-error';
import {
  SaveServiceProviderEmployeeRequest,
  ServiceProviderEmployee
} from '../models/service-provider-employee.model';
import { ProviderEmployeeService } from '../services/provider-employee.service';

@Component({
  selector: 'app-provider-employee-list',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe],
  templateUrl: './provider-employee-list.component.html',
  styleUrl: './provider-employee-list.component.scss'
})
export class ProviderEmployeeListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(ProviderEmployeeService);

  readonly serviceProviderId = this.route.snapshot.paramMap.get('serviceProviderId') ?? '';
  readonly employees = signal<ServiceProviderEmployee[]>([]);
  readonly search = signal('');
  readonly editingId = signal<string | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  readonly roles = [
    { value: 'SupportWorker', label: 'Support worker' },
    { value: 'RegisteredNurse', label: 'Registered nurse' },
    { value: 'Therapist', label: 'Therapist' },
    { value: 'Coordinator', label: 'Coordinator' },
    { value: 'Other', label: 'Other' }
  ] as const;

  readonly filtered = computed(() => {
    const query = this.search().trim().toLowerCase();
    if (!query) {
      return this.employees();
    }

    return this.employees().filter((employee) =>
      `${employee.employeeNumber} ${employee.firstName} ${employee.lastName} ${employee.email} ${employee.role}`
        .toLowerCase()
        .includes(query)
    );
  });

  readonly form = this.fb.nonNullable.group({
    employeeNumber: ['', [Validators.required, Validators.maxLength(50)]],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(250)]],
    phone: ['', Validators.maxLength(30)],
    role: ['SupportWorker', Validators.required],
    qualifications: ['', Validators.maxLength(500)],
    defaultHourlyRate: ['', [Validators.required, Validators.pattern(/^\d+(\.\d{1,2})?$/)]]
  });

  constructor() {
    this.load();
  }

  load(): void {
    if (!this.serviceProviderId) {
      this.error.set('Service provider id is missing from the route.');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.service
      .getAll(this.serviceProviderId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (employees) => this.employees.set(employees),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  edit(employee: ServiceProviderEmployee): void {
    this.editingId.set(employee.id);
    this.error.set('');
    this.message.set('');

    this.form.setValue({
      employeeNumber: employee.employeeNumber,
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      phone: employee.phone ?? '',
      role: employee.role,
      qualifications: employee.qualifications ?? '',
      defaultHourlyRate: employee.defaultHourlyRate?.toString() ?? ''
    });
  }

  reset(): void {
    this.editingId.set(null);
    this.form.reset({
      employeeNumber: '',
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      role: 'SupportWorker',
      qualifications: '',
      defaultHourlyRate: ''
    });
  }

  save(): void {
    this.error.set('');
    this.message.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('Please complete the required employee fields.');
      return;
    }

    const raw = this.form.getRawValue();
    const employeePayRate = this.optionalNumber(raw.defaultHourlyRate);
    if (employeePayRate === null || employeePayRate <= 0) {
      this.error.set('Employee pay rate must be greater than zero.');
      return;
    }

    const request: SaveServiceProviderEmployeeRequest = {
      employeeNumber: raw.employeeNumber.trim(),
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      email: raw.email.trim(),
      phone: raw.phone.trim() || null,
      role: raw.role,
      qualifications: raw.qualifications.trim() || null,
      defaultHourlyRate: employeePayRate
    };

    const editingId = this.editingId();
    const request$ = editingId
      ? this.service.update(this.serviceProviderId, editingId, request)
      : this.service.create(this.serviceProviderId, request);

    this.saving.set(true);
    request$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.message.set(editingId ? 'Employee updated.' : 'Employee added.');
          this.reset();
          this.load();
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  toggle(employee: ServiceProviderEmployee): void {
    this.error.set('');
    this.message.set('');

    this.service
      .setActive(this.serviceProviderId, employee.id, !employee.isActive)
      .subscribe({
        next: () => {
          this.message.set(employee.isActive ? 'Employee deactivated.' : 'Employee activated.');
          this.load();
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

private optionalNumber(
  value: string | number | null | undefined
): number | null {
  if (
    value === null ||
    value === undefined ||
    value === ''
  ) {
    return null;
  }

  const numberValue = Number(value);

  return Number.isNaN(numberValue)
    ? null
    : numberValue;
}
}
