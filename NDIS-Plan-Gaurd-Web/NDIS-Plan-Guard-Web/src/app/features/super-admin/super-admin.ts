import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { apiErrorMessage } from '../../core/services/api-error';
import { SuperAdminService } from './super-admin.service';
import { UserSummaryResponse } from './super-admin.models';

@Component({
  selector: 'app-super-admin',
  imports: [ReactiveFormsModule],
  templateUrl: './super-admin.html',
  styleUrl: './super-admin.scss'
})
export class SuperAdminComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SuperAdminService);

  readonly users = signal<UserSummaryResponse[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    phoneNumber: ['']
  });

  constructor() { this.loadUsers(); }

  loadUsers(): void {
    this.loading.set(true);
    this.service.getUsers().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (users) => this.users.set(users),
      error: (error) => this.error.set(apiErrorMessage(error))
    });
  }

  createAdmin(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const value = this.form.getRawValue();
    this.error.set('');
    this.service.createAdmin({ ...value, phoneNumber: value.phoneNumber.trim() || null }).subscribe({
      next: (response) => { this.message.set(response.message); this.form.reset(); this.loadUsers(); },
      error: (error) => this.error.set(apiErrorMessage(error))
    });
  }

  setActive(user: UserSummaryResponse, active: boolean): void {
    const request = active ? this.service.activateUser(user.userProfileId) : this.service.disableUser(user.userProfileId);
    request.subscribe({ next: () => this.loadUsers(), error: (error) => this.error.set(apiErrorMessage(error)) });
  }
}
