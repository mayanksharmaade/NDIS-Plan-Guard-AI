import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../core/services/api-error';
import { ProviderService } from './provider.service';
import { ServiceProviderProfileResponse } from './provider.models';

@Component({
  selector: 'app-provider-profile',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './provider-profile.html',
  styleUrl: './provider-profile.scss'
})
export class ProviderProfileComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ProviderService);

  readonly profile = signal<ServiceProviderProfileResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    legalName: ['', Validators.required],
    tradingName: [''],
    abn: ['', [Validators.required, Validators.pattern(/^\d{11}$/)]],
    ndisRegistrationNumber: ['']
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');

    this.service
      .getMine()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (profile) => {
          this.profile.set(profile);
          this.form.setValue({
            firstName: profile.firstName,
            lastName: profile.lastName,
            legalName: profile.legalName,
            tradingName: profile.tradingName ?? '',
            abn: profile.abn,
            ndisRegistrationNumber: profile.ndisRegistrationNumber ?? ''
          });
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.service
      .updateMine({
        ...value,
        tradingName: value.tradingName.trim() || null,
        ndisRegistrationNumber: value.ndisRegistrationNumber.trim() || null
      })
      .subscribe({
        next: () => {
          this.message.set('Profile updated.');
          this.load();
        },
        error: (error: unknown) => this.error.set(apiErrorMessage(error))
      });
  }
}
