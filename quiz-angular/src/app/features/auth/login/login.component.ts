import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginRequest } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-md-5">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h2 class="card-title mb-4">Sign In</h2>

            @if (error()) {
              <div class="alert alert-danger">{{ error() }}</div>
            }

            <form [formGroup]="form" (ngSubmit)="submit()">
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

              <div class="mb-4">
                <label for="password" class="form-label">Password</label>
                <input
                  id="password"
                  type="password"
                  class="form-control"
                  [class.is-invalid]="form.get('password')?.invalid && form.get('password')?.touched"
                  formControlName="password"
                  autocomplete="current-password"
                />
                @if (form.get('password')?.errors?.['required'] && form.get('password')?.touched) {
                  <div class="invalid-feedback">Password is required.</div>
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
                Sign In
              </button>
            </form>

            <p class="mt-3 text-center text-muted mb-0">
              No account? <a routerLink="/register">Register</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.authService.login(this.form.value as LoginRequest).subscribe({
      next: () => this.router.navigate(['/quizzes']),
      error: (err: HttpErrorResponse) => {
        this.error.set(err.error?.message ?? 'Login failed. Check your credentials.');
        this.loading.set(false);
      }
    });
  }
}
