import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/auth/auth.service';
import { RegisterRequest } from '../../../core/models/auth.models';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordsMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-md-5">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="card-title mb-4 h2">Create Account</h1>

            @if (error()) {
              <div class="alert alert-danger">{{ error() }}</div>
            }

            <form [formGroup]="form" (ngSubmit)="submit()">
              <div class="mb-3">
                <label for="displayName" class="form-label">Display Name</label>
                <input
                  id="displayName"
                  type="text"
                  class="form-control"
                  [class.is-invalid]="form.get('displayName')?.invalid && form.get('displayName')?.touched"
                  formControlName="displayName"
                  autocomplete="nickname"
                />
                @if (form.get('displayName')?.errors?.['required'] && form.get('displayName')?.touched) {
                  <div class="invalid-feedback">Display name is required.</div>
                }
                @if (form.get('displayName')?.errors?.['minlength'] && form.get('displayName')?.touched) {
                  <div class="invalid-feedback">Minimum 2 characters.</div>
                }
                @if (form.get('displayName')?.errors?.['maxlength'] && form.get('displayName')?.touched) {
                  <div class="invalid-feedback">Maximum 50 characters.</div>
                }
              </div>

              <div class="mb-3">
                <label for="email" class="form-label">Email</label>
                <input
                  id="email"
                  type="email"
                  class="form-control"
                  [class.is-invalid]="form.get('email')?.invalid && form.get('email')?.touched"
                  formControlName="email"
                  autocomplete="email"
                />
                @if (form.get('email')?.errors?.['required'] && form.get('email')?.touched) {
                  <div class="invalid-feedback">Email is required.</div>
                }
                @if (form.get('email')?.errors?.['email'] && form.get('email')?.touched) {
                  <div class="invalid-feedback">Enter a valid email address.</div>
                }
              </div>

              <div class="mb-3">
                <label for="password" class="form-label">Password</label>
                <input
                  id="password"
                  type="password"
                  class="form-control"
                  [class.is-invalid]="form.get('password')?.invalid && form.get('password')?.touched"
                  formControlName="password"
                  autocomplete="new-password"
                />
                @if (form.get('password')?.errors?.['required'] && form.get('password')?.touched) {
                  <div class="invalid-feedback">Password is required.</div>
                }
                @if (form.get('password')?.errors?.['minlength'] && form.get('password')?.touched) {
                  <div class="invalid-feedback">Minimum 8 characters.</div>
                }
              </div>

              <div class="mb-4">
                <label for="confirmPassword" class="form-label">Confirm Password</label>
                <input
                  id="confirmPassword"
                  type="password"
                  class="form-control"
                  [class.is-invalid]="(form.get('confirmPassword')?.touched && form.errors?.['passwordsMismatch'])"
                  formControlName="confirmPassword"
                  autocomplete="new-password"
                />
                @if (form.get('confirmPassword')?.touched && form.errors?.['passwordsMismatch']) {
                  <div class="invalid-feedback d-block">Passwords do not match.</div>
                }
              </div>

              <button
                type="submit"
                class="btn btn-primary w-100"
                [disabled]="loading()"
              >
                @if (loading()) {
                  <span class="spinner-border spinner-border-sm me-2"></span>
                }
                Create Account
              </button>
            </form>

            <p class="mt-3 text-center text-muted mb-0">
              Already have an account? <a routerLink="/login">Sign in</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordsMatch });

  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    const { displayName, email, password } = this.form.value;
    this.authService.register({ displayName: displayName!, email: email!, password: password! } as RegisterRequest).subscribe({
      next: () => this.router.navigate(['/quizzes']),
      error: (err: HttpErrorResponse) => {
        this.error.set(err.error?.message ?? 'Registration failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
