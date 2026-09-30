import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, forkJoin, switchMap } from 'rxjs';
import { apiErrorMessage } from '../../../core/services/api-error';
import { SUPPORT_SERVICES, supportServiceName } from '../../../shared/constants/support-services';
import { ParticipantResponse } from '../../participants/participant.models';
import { ParticipantService } from '../../participants/participant.service';
import { ProviderEmployeeService } from '../../provider-employees/services/provider-employee.service';
import { ServiceProviderEmployee } from '../../provider-employees/models/service-provider-employee.model';
import { ProviderService } from '../../service-providers/provider.service';
import {
  CreateParticipantServiceAssignmentRequest,
  CreateServiceDeliveryRequest,
  ParticipantServiceAssignment,
  ServiceDelivery
} from '../models/participant-service.model';
import { ParticipantServiceService } from '../services/participant-service.service';

@Component({
  selector: 'app-participant-service-list',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './participant-service-list.component.html',
  styleUrl: './participant-service-list.component.scss'
})
export class ParticipantServiceListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly participantService = inject(ParticipantService);
  private readonly providerService = inject(ProviderService);
  private readonly employeeService = inject(ProviderEmployeeService);
  private readonly service = inject(ParticipantServiceService);

  readonly participantId = this.route.snapshot.paramMap.get('participantId') ?? '';
  readonly participant = signal<ParticipantResponse | null>(null);
  readonly employees = signal<ServiceProviderEmployee[]>([]);
  readonly assignments = signal<ParticipantServiceAssignment[]>([]);
  readonly deliveries = signal<ServiceDelivery[]>([]);
  readonly loading = signal(false);
  readonly savingAssignment = signal(false);
  readonly savingDelivery = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  readonly supportServices = SUPPORT_SERVICES;
  readonly supportName = supportServiceName;

  readonly activeEmployees = computed(() => this.employees().filter((employee) => employee.isActive));
  readonly activeAssignments = computed(() => this.assignments().filter((assignment) => assignment.isActive));

  readonly selectedAssignment = computed(() => {
    const id = this.deliveryForm.controls.participantServiceAssignmentId.value;
    return this.assignments().find((item) => item.id === id) ?? null;
  });

  readonly selectedEmployee = computed(() => {
    const id = this.assignmentForm.controls.serviceProviderEmployeeId.value;
    return this.employees().find((item) => item.id === id) ?? null;
  });

  readonly assignmentForm = this.fb.nonNullable.group({
    serviceProviderEmployeeId: ['', Validators.required],
    supportCategory: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: [''],
    agreedHourlyRate: ['', [Validators.required, Validators.pattern(/^\d+(\.\d{1,2})?$/)]]
  });

  readonly deliveryForm = this.fb.nonNullable.group({
    participantServiceAssignmentId: ['', Validators.required],
    serviceStartLocal: ['', Validators.required],
    serviceEndLocal: ['', Validators.required],
    serviceLocation: ['', Validators.maxLength(300)],
    notes: ['', Validators.maxLength(1000)]
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
      profile: this.providerService.getMine(),
      assignments: this.service.getAssignments(this.participantId),
      deliveries: this.service.getDeliveries(this.participantId)
    })
      .pipe(
        switchMap(({ participant, profile, assignments, deliveries }) => {
          this.participant.set(participant);
          this.assignments.set(assignments);
          this.deliveries.set(deliveries);
          return this.employeeService.getAll(profile.serviceProviderId);
        }),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (employees) => this.employees.set(employees),
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  assignEmployee(): void {
    this.error.set('');
    this.message.set('');

    if (this.assignmentForm.invalid) {
      this.assignmentForm.markAllAsTouched();
      this.error.set('Please complete the required assignment fields.');
      return;
    }

    const raw = this.assignmentForm.getRawValue();
    if (raw.endDate && raw.endDate < raw.startDate) {
      this.error.set('Assignment end date cannot be before the start date.');
      return;
    }

    const agreedHourlyRate = this.optionalNumber(raw.agreedHourlyRate);
    if (agreedHourlyRate === null || agreedHourlyRate <= 0) {
      this.error.set('Agreed participant billing rate must be greater than zero.');
      return;
    }

    const employee = this.employees().find((item) => item.id === raw.serviceProviderEmployeeId);
    if (!employee?.defaultHourlyRate || employee.defaultHourlyRate <= 0) {
      this.error.set('The selected employee must have an employee pay rate before assignment.');
      return;
    }

    const request: CreateParticipantServiceAssignmentRequest = {
      serviceProviderEmployeeId: raw.serviceProviderEmployeeId,
      supportCategory: raw.supportCategory,
      startDate: raw.startDate,
      endDate: raw.endDate || null,
      agreedHourlyRate
    };

    this.savingAssignment.set(true);
    this.service
      .assignEmployee(this.participantId, request)
      .pipe(finalize(() => this.savingAssignment.set(false)))
      .subscribe({
        next: () => {
          this.message.set('Employee assigned to participant.');
          this.assignmentForm.reset({
            serviceProviderEmployeeId: '',
            supportCategory: '',
            startDate: '',
            endDate: '',
            agreedHourlyRate: ''
          });
          this.load();
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  recordDelivery(): void {
    this.error.set('');
    this.message.set('');

    if (this.deliveryForm.invalid) {
      this.deliveryForm.markAllAsTouched();
      this.error.set('Select an assignment and complete the service start/end times.');
      return;
    }

    const raw = this.deliveryForm.getRawValue();
    const start = new Date(raw.serviceStartLocal);
    const end = new Date(raw.serviceEndLocal);

    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
      this.error.set('Enter valid service start and end times.');
      return;
    }

    if (end <= start) {
      this.error.set('Service end must be after service start.');
      return;
    }

    const request: CreateServiceDeliveryRequest = {
      participantServiceAssignmentId: raw.participantServiceAssignmentId,
      serviceStartUtc: start.toISOString(),
      serviceEndUtc: end.toISOString(),
      serviceLocation: raw.serviceLocation.trim() || null,
      notes: raw.notes.trim() || null
    };

    this.savingDelivery.set(true);
    this.service
      .recordDelivery(this.participantId, request)
      .pipe(finalize(() => this.savingDelivery.set(false)))
      .subscribe({
        next: () => {
          this.message.set('Service delivery recorded using the agreed participant billing rate.');
          this.deliveryForm.reset({
            participantServiceAssignmentId: '',
            serviceStartLocal: '',
            serviceEndLocal: '',
            serviceLocation: '',
            notes: ''
          });
          this.load();
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  private optionalNumber(value: string | number | null | undefined): number | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const numberValue = Number(value);
    return Number.isNaN(numberValue) ? null : numberValue;
  }
}
