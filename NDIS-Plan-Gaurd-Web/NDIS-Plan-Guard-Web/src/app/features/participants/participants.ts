import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  inject,
  signal
} from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { RouterLink } from '@angular/router';

import {
  finalize,
  Observable
} from 'rxjs';

import {
  apiErrorMessage
} from '../../core/services/api-error';

import {
  ParticipantResponse,
  UpdateParticipantRequest
} from './participant.models';

import {
  ParticipantService
} from './participant.service';


@Component({
  selector: 'app-participants',

  imports: [
    ReactiveFormsModule,
    DatePipe,
    RouterLink
  ],

  templateUrl: './participants.html',
  styleUrl: './participants.scss'
})
export class ParticipantsComponent {

  private readonly fb =
    inject(FormBuilder);

  private readonly service =
    inject(ParticipantService);


  readonly participants =
    signal<ParticipantResponse[]>([]);

  readonly search =
    signal('');

  readonly editingId =
    signal<string | null>(null);

  readonly loading =
    signal(false);

  readonly error =
    signal('');

  readonly message =
    signal('');


  readonly filtered = computed(() => {

    const query =
      this.search()
        .trim()
        .toLowerCase();

    if (!query) {
      return this.participants();
    }

    return this.participants()
      .filter((participant) =>
        `${participant.ndisNumber}
         ${participant.firstName}
         ${participant.lastName}
         ${participant.email ?? ''}`
          .toLowerCase()
          .includes(query)
      );
  });


  readonly form =
    this.fb.nonNullable.group({

      ndisNumber: [
        '',
        Validators.required
      ],

      firstName: [
        '',
        Validators.required
      ],

      lastName: [
        '',
        Validators.required
      ],

      dateOfBirth: [''],

      email: [
        '',
        Validators.email
      ],

      phoneNumber: [''],

      planStartDate: [''],

      planEndDate: [''],

      emergencyContactName: [''],

      emergencyContactRelationship: [''],

      emergencyContactPhoneNumber: ['']
    });


  constructor() {
    this.load();
  }


  load(): void {

    this.loading.set(true);

    this.error.set('');


    this.service
      .list()
      .pipe(
        finalize(
          () => this.loading.set(false)
        )
      )
      .subscribe({

        next: (participants) => {

          this.participants.set(
            participants
          );
        },

        error: (e: unknown) => {

          this.error.set(
            apiErrorMessage(e)
          );
        }
      });
  }


  edit(
    participant: ParticipantResponse
  ): void {

    this.editingId.set(
      participant.id
    );

    this.error.set('');

    this.message.set('');


    this.form.setValue({

      ndisNumber:
        participant.ndisNumber,

      firstName:
        participant.firstName,

      lastName:
        participant.lastName,

      dateOfBirth:
        this.dateInput(
          participant.dateOfBirth
        ),

      email:
        participant.email ?? '',

      phoneNumber:
        participant.phoneNumber ?? '',

      planStartDate:
        this.dateInput(
          participant.planStartDate
        ),

      planEndDate:
        this.dateInput(
          participant.planEndDate
        ),

      emergencyContactName:
        participant.emergencyContactName
        ?? '',

      emergencyContactRelationship:
        participant.emergencyContactRelationship
        ?? '',

      emergencyContactPhoneNumber:
        participant.emergencyContactPhoneNumber
        ?? ''
    });


    this.form.controls
      .ndisNumber
      .disable();
  }


  reset(): void {

    this.editingId.set(null);


    this.form.reset({

      ndisNumber: '',

      firstName: '',

      lastName: '',

      dateOfBirth: '',

      email: '',

      phoneNumber: '',

      planStartDate: '',

      planEndDate: '',

      emergencyContactName: '',

      emergencyContactRelationship: '',

      emergencyContactPhoneNumber: ''
    });


    this.form.controls
      .ndisNumber
      .enable();
  }


  save(): void {

    this.error.set('');

    this.message.set('');


    if (this.form.invalid) {

      this.form.markAllAsTouched();

      return;
    }


    const raw =
      this.form.getRawValue();


    const id =
      this.editingId();


    /*
     * Budget UI will be implemented next.
     *
     * Until then:
     * - existing participant -> preserve existing budget
     * - new participant      -> null
     *
     * This prevents editing emergency contact
     * details from accidentally clearing an
     * existing PlanTotalBudget.
     */
    const existingParticipant =
      id
        ? this.participants()
            .find(
              participant =>
                participant.id === id
            )
        : null;


    const common:
      UpdateParticipantRequest = {

      firstName:
        raw.firstName.trim(),

      lastName:
        raw.lastName.trim(),

      dateOfBirth:
        this.nullDate(
          raw.dateOfBirth
        ),

      email:
        raw.email.trim()
        || null,

      phoneNumber:
        raw.phoneNumber.trim()
        || null,

      planStartDate:
        this.nullDate(
          raw.planStartDate
        ),

      planEndDate:
        this.nullDate(
          raw.planEndDate
        ),

      emergencyContactName:
        raw.emergencyContactName
          .trim()
        || null,

      emergencyContactRelationship:
        raw.emergencyContactRelationship
          .trim()
        || null,

      emergencyContactPhoneNumber:
        raw.emergencyContactPhoneNumber
          .trim()
        || null,

      planTotalBudget:
        existingParticipant
          ?.planTotalBudget
        ?? null
    };


    const request$:
      Observable<unknown> = id

        ? this.service.update(
            id,
            common
          )

        : this.service.create({

            ndisNumber:
              raw.ndisNumber.trim(),

            ...common
          });


    request$.subscribe({

      next: () => {

        this.message.set(
          id
            ? 'Participant updated.'
            : 'Participant created.'
        );


        this.reset();

        this.load();
      },

      error: (e: unknown) => {

        this.error.set(
          apiErrorMessage(e)
        );
      }
    });
  }


  deactivate(
    participant: ParticipantResponse
  ): void {

    this.error.set('');

    this.message.set('');


    this.service
      .deactivate(
        participant.id
      )
      .subscribe({

        next: () => {

          this.message.set(
            'Participant deactivated.'
          );

          this.load();
        },

        error: (e: unknown) => {

          this.error.set(
            apiErrorMessage(e)
          );
        }
      });
  }


  private nullDate(
    value: string
  ): string | null {

    const trimmed =
      value.trim();

    return trimmed || null;
  }


  private dateInput(
    value: string | null
  ): string {

    return value
      ? value.substring(0, 10)
      : '';
  }
}