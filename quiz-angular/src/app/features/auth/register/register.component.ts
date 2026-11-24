import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
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
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .auth-shell {
      min-height: calc(100dvh - 120px);
      display: grid;
      place-items: center;
      padding: var(--s-6) var(--s-4);
    }
    .auth-card {
      position: relative;
      width: 100%;
      max-width: 460px;
      background: linear-gradient(145deg, var(--surface), var(--bg-elev));
      border: 1px solid var(--border);
      border-radius: var(--r-xl);
      padding: var(--s-7) var(--s-6);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }
    .auth-card::before {
      content: '';
      position: absolute; top: 0; left: 0; right: 0; height: 3px;
      background: linear-gradient(90deg, var(--violet), var(--cyan));
    }
    .auth-eyebrow {
      display: block; color: var(--violet-hi);
      font-family: var(--font-display); font-weight: 600;
      font-size: 11px; letter-spacing: 0.15em; text-transform: uppercase;
      margin-bottom: var(--s-2);
    }
    .auth-card h1 {
      margin: 0 0 var(--s-5); font-size: 24px;
      background: linear-gradient(135deg, var(--violet), var(--cyan));
      -webkit-background-clip: text; background-clip: text;
      -webkit-text-fill-color: transparent; color: transparent;
    }
    .auth-field { margin-bottom: var(--s-4); }
    .auth-field label {
      display: block; font-size: 12px; font-weight: 600;
      letter-spacing: 0.04em; color: var(--text-dim); margin-bottom: 6px;
    }
    .auth-input {
      width: 100%; padding: 12px 14px;
      background: var(--surface); border: 1px solid var(--border);
      border-radius: var(--r-md); color: var(--text);
      font: inherit; font-size: 15px;
      transition: border-color var(--t-mid), box-shadow var(--t-mid);
    }
    .auth-input:focus {
      outline: none; border-color: var(--violet);
      box-shadow: 0 0 0 3px color-mix(in oklab, var(--violet) 25%, transparent);
    }
    .auth-input.is-invalid { border-color: var(--danger); }
    .field-error { margin-top: 6px; color: var(--danger); font-size: 12px; }
    .alert-err {
      padding: var(--s-3) var(--s-4); border-radius: var(--r-md);
      background: color-mix(in oklab, var(--danger) 15%, var(--surface));
      border: 1px solid color-mix(in oklab, var(--danger) 40%, var(--border));
      color: var(--danger); margin-bottom: var(--s-4); font-size: 14px;
    }
    .auth-cta { width: 100%; margin-top: var(--s-2); }
    .auth-foot { margin: var(--s-5) 0 0; text-align: center; color: var(--text-dim); font-size: 13px; }
    .auth-foot a { color: var(--cyan); }
    .spinner {
      width: 14px; height: 14px;
      border: 2px solid currentColor; border-top-color: transparent;
      border-radius: 50%; display: inline-block;
      margin-right: 8px; vertical-align: -2px;
      animation: spin 0.7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (prefers-reduced-motion: reduce) { .spinner { animation-duration: 2s; } }
  `],
  template: `
    <div class="auth-shell">
      <div class="auth-card">
        <span class="auth-eyebrow">Join the rush</span>
        <h1>Create your account</h1>

        @if (error()) {
          <div class="alert-err" role="alert">{{ error() }}</div>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div class="auth-field">
            <label for="displayName">Display name</label>
            <input id="displayName" type="text" class="auth-input"
                   [class.is-invalid]="form.get('displayName')?.invalid && form.get('displayName')?.touched"
                   formControlName="displayName" autocomplete="nickname" />
            @if (form.get('displayName')?.errors?.['required'] && form.get('displayName')?.touched) {
              <div class="field-error">Display name is required.</div>
            }
            @if (form.get('displayName')?.errors?.['minlength'] && form.get('displayName')?.touched) {
              <div class="field-error">Minimum 2 characters.</div>
            }
            @if (form.get('displayName')?.errors?.['maxlength'] && form.get('displayName')?.touched) {
              <div class="field-error">Maximum 50 characters.</div>
            }
          </div>

          <div class="auth-field">
            <label for="email">Email</label>
            <input id="email" type="email" class="auth-input"
                   [class.is-invalid]="form.get('email')?.invalid && form.get('email')?.touched"
                   formControlName="email" autocomplete="email" />
            @if (form.get('email')?.errors?.['required'] && form.get('email')?.touched) {
              <div class="field-error">Email is required.</div>
            }
            @if (form.get('email')?.errors?.['email'] && form.get('email')?.touched) {
              <div class="field-error">Enter a valid email address.</div>
            }
          </div>

          <div class="auth-field">
            <label for="password">Password</label>
            <input id="password" type="password" class="auth-input"
                   [class.is-invalid]="form.get('password')?.invalid && form.get('password')?.touched"
                   formControlName="password" autocomplete="new-password" />
            @if (form.get('password')?.errors?.['required'] && form.get('password')?.touched) {
              <div class="field-error">Password is required.</div>
            }
            @if (form.get('password')?.errors?.['minlength'] && form.get('password')?.touched) {
              <div class="field-error">Minimum 8 characters.</div>
            }
          </div>

          <div class="auth-field">
            <label for="confirmPassword">Confirm password</label>
            <input id="confirmPassword" type="password" class="auth-input"
                   [class.is-invalid]="form.get('confirmPassword')?.touched && form.errors?.['passwordsMismatch']"
                   formControlName="confirmPassword" autocomplete="new-password" />
            @if (form.get('confirmPassword')?.touched && form.errors?.['passwordsMismatch']) {
              <div class="field-error">Passwords do not match.</div>
            }
          </div>

          <button type="submit" class="btn-neon auth-cta" [disabled]="loading()">
            @if (loading()) { <span class="spinner" aria-hidden="true"></span> }
            Create account
          </button>
        </form>

        <p class="auth-foot">Already have an account? <a routerLink="/login">Sign in</a></p>
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
