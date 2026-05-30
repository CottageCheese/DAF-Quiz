import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container">
        <a class="navbar-brand" routerLink="/">DAF Quiz</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <li class="nav-item">
              <a class="nav-link" routerLink="/leaderboard" routerLinkActive="active">Leaderboard</a>
            </li>
            @if (authService.isLoggedIn()) {
              <li class="nav-item">
                <a class="nav-link" routerLink="/quizzes" routerLinkActive="active">Quizzes</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" routerLink="/history" routerLinkActive="active">My History</a>
              </li>
            }
          </ul>
          <ul class="navbar-nav">
            @if (authService.isLoggedIn()) {
              <li class="nav-item">
                <span class="nav-link text-light opacity-75">
                  {{ authService.currentUser()?.displayName }}
                </span>
              </li>
              <li class="nav-item">
                <button class="btn btn-outline-light btn-sm ms-2" (click)="authService.logout()">
                  Logout
                </button>
              </li>
            } @else {
              <li class="nav-item">
                <a class="nav-link" routerLink="/login" routerLinkActive="active">Login</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" routerLink="/register" routerLinkActive="active">Register</a>
              </li>
            }
          </ul>
        </div>
      </div>
    </nav>
  `
})
export class NavbarComponent {
  readonly authService = inject(AuthService);
}
