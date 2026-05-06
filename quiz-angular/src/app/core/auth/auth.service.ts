import { Injectable, Signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, EMPTY, Observable, throwError } from 'rxjs';
import { catchError, finalize, map, shareReplay, tap } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthResponse, AuthUser, JwtPayload, LoginRequest, RegisterRequest } from '../models/auth.models';
import { TokenStorageService } from './token-storage.service';
import { api } from '../api';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorageService);
  private readonly router = inject(Router);

  private readonly _currentUser$ = new BehaviorSubject<AuthUser | null>(
    this.resolveUserFromStorage()
  );

  readonly currentUser$: Observable<AuthUser | null> = this._currentUser$.asObservable();
  readonly currentUser: Signal<AuthUser | null> = toSignal(this._currentUser$, { initialValue: null });
  readonly isLoggedIn: Signal<boolean> = computed(() => this.currentUser() !== null);

  private _refreshInProgress$: Observable<AuthResponse> | null = null;

  register(req: RegisterRequest): Observable<void> {
    return this.http.post<AuthResponse>(api('/api/auth/register'), req).pipe(
      tap(res => this.handleAuthResponse(res)),
      map(() => void 0)
    );
  }

  login(req: LoginRequest): Observable<void> {
    return this.http.post<AuthResponse>(api('/api/auth/login'), req).pipe(
      tap(res => this.handleAuthResponse(res)),
      map(() => void 0)
    );
  }

  logout(): void {
    const refreshToken = this.storage.getRefreshToken();
    if (refreshToken) {
      this.http.post(api('/api/auth/revoke'), { refreshToken }).pipe(
        catchError(() => EMPTY)
      ).subscribe();
    }
    this.storage.clear();
    this._currentUser$.next(null);
    this.router.navigate(['/login']);
  }

  refreshTokens(): Observable<AuthResponse> {
    if (!this._refreshInProgress$) {
      const refreshToken = this.storage.getRefreshToken();
      if (!refreshToken) {
        this.logout();
        return throwError(() => new Error('No refresh token available'));
      }
      this._refreshInProgress$ = this.http
        .post<AuthResponse>(api('/api/auth/refresh'), { refreshToken })
        .pipe(
          tap(res => this.handleAuthResponse(res)),
          catchError(err => {
            this.logout();
            return throwError(() => err);
          }),
          shareReplay(1),
          finalize(() => { this._refreshInProgress$ = null; })
        );
    }
    return this._refreshInProgress$;
  }

  private handleAuthResponse(res: AuthResponse): void {
    this.storage.saveTokens(res);
    const user = this.decodeUser(res.accessToken);
    this._currentUser$.next(user);
  }

  private resolveUserFromStorage(): AuthUser | null {
    const token = this.storage.getAccessToken();
    if (!token || this.storage.isAccessTokenExpired()) return null;
    return this.decodeUser(token);
  }

  private decodeUser(token: string): AuthUser | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1])) as JwtPayload;
      return {
        email: payload.email ?? payload.sub,
        displayName: payload.display_name ?? payload.email ?? 'User'
      };
    } catch {
      return null;
    }
  }
}
