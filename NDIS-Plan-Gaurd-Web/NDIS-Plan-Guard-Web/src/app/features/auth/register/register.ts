import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { apiErrorMessage } from '../../../core/services/api-error';

function abnValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const rawValue = String(control.value ?? '');

    const digitsOnly = rawValue.replace(/\s/g, '');

    if (!digitsOnly) {
      return null;
    }

    return /^\d{11}$/.test(digitsOnly)
      ? null
      : { invalidAbnFormat: true };
  };
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly success = signal('');

  readonly form = this.fb.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],

    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8)
      ]
    ],

    firstName: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],

    lastName: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],

    legalName: [
      '',
      [
        Validators.required,
        Validators.maxLength(200)
      ]
    ],

    tradingName: [
      '',
      Validators.maxLength(200)
    ],

    abn: [
      '',
      [
        Validators.required,
        abnValidator()
      ]
    ],

    ndisRegistrationNumber: [
      '',
      Validators.maxLength(100)
    ],

    phoneNumber: ['']
  });

  submit(): void {
    this.error.set('');
    this.success.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.error.set(
        'Please check the highlighted fields and complete all required information.'
      );

      console.log('Registration form is invalid.');

      Object.entries(this.form.controls).forEach(
        ([name, control]) => {
          if (control.invalid) {
            console.log(
              `Invalid field: ${name}`,
              control.errors
            );
          }
        }
      );

      return;
    }

    const value = this.form.getRawValue();

    const cleanAbn = value.abn.replace(/\s/g, '');

    this.loading.set(true);

    this.auth
      .registerServiceProvider({
        email: value.email.trim(),
        password: value.password,
        firstName: value.firstName.trim(),
        lastName: value.lastName.trim(),
        legalName: value.legalName.trim(),
        tradingName: value.tradingName.trim() || null,
        abn: cleanAbn,
        ndisRegistrationNumber:
          value.ndisRegistrationNumber.trim() || null,
        phoneNumber:
          value.phoneNumber.trim() || null
      })
      .pipe(
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.success.set(response.message);

          this.form.reset();
        },

        error: (error: unknown) => {
          this.error.set(
            apiErrorMessage(error)
          );
        }
      });
  }
}