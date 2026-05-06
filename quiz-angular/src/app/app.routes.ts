import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { alreadyLoggedInGuard } from './core/guards/already-logged-in.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'quizzes', pathMatch: 'full' },
  {
    path: 'login',
    canActivate: [alreadyLoggedInGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [alreadyLoggedInGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'leaderboard',
    loadComponent: () =>
      import('./features/leaderboard/leaderboard.component').then(m => m.LeaderboardComponent)
  },
  {
    path: 'quizzes',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/quiz/quiz.routes').then(m => m.QUIZ_ROUTES)
  },
  {
    path: 'history',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/history/history.component').then(m => m.HistoryComponent)
  },
  { path: '**', redirectTo: 'quizzes' }
];
