import { Component, inject, ChangeDetectionStrategy, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `
    <header class="topbar" role="banner">
      <a class="brand" routerLink="/" aria-label="TriviaRush home">
        <span class="brand-mark" aria-hidden="true"></span>
        TriviaRush
      </a>

      <nav class="topbar-nav" aria-label="Primary">
        @if (authService.isLoggedIn()) {
          <a routerLink="/quizzes" routerLinkActive="active">Quizzes</a>
          <a routerLink="/history" routerLinkActive="active">History</a>
        }
        <a routerLink="/leaderboard" routerLinkActive="active">Leaderboard</a>
      </nav>

      <div class="topbar-actions">
        <button type="button"
                class="theme-toggle"
                [attr.aria-label]="theme() === 'light' ? 'Switch to dark theme' : 'Switch to light theme'"
                [attr.aria-pressed]="theme() === 'light'"
                (click)="toggleTheme()">
          <svg class="icon-moon" width="16" height="16" viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
          </svg>
          <svg class="icon-sun" width="16" height="16" viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <circle cx="12" cy="12" r="4"/>
            <path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41"/>
          </svg>
        </button>

        @if (authService.isLoggedIn()) {
          <span class="user-chip" [attr.aria-label]="'Signed in as ' + (authService.currentUser()?.displayName || '')">
            <span class="avatar" aria-hidden="true">{{ monogram() }}</span>
            {{ authService.currentUser()?.displayName }}
          </span>
          <button type="button" class="topbar-link" (click)="authService.logout()">Sign out</button>
        } @else {
          <a class="topbar-link" routerLink="/login">Sign in</a>
          <a class="topbar-link" routerLink="/register">Register</a>
        }
      </div>
    </header>
  `
})
export class NavbarComponent {
  readonly authService = inject(AuthService);
  readonly theme = signal<'dark' | 'light'>(this.readTheme());

  monogram(): string {
    const name = this.authService.currentUser()?.displayName ?? '';
    return name ? name.charAt(0).toUpperCase() : '?';
  }

  toggleTheme(): void {
    const next = this.theme() === 'light' ? 'dark' : 'light';
    this.theme.set(next);
    document.documentElement.setAttribute('data-theme', next);
    const meta = document.querySelector('meta[name="color-scheme"]');
    meta?.setAttribute('content', next);
    try { localStorage.setItem('theme', next); } catch { /* ignore */ }
  }

  private readTheme(): 'dark' | 'light' {
    const t = document.documentElement.getAttribute('data-theme');
    return t === 'light' ? 'light' : 'dark';
  }
}
