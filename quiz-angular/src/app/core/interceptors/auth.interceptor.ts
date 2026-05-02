import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { TokenStorageService } from '../auth/token-storage.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);
  const authService = inject(AuthService);

  // Never intercept auth endpoints — avoid infinite refresh loops
  if (req.url.includes('/api/auth/')) {
    return next(req);
  }

  const sendWithToken = (token: string) =>
    next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));

  // Proactive refresh: token already expired before sending
  if (tokenStorage.isAccessTokenExpired()) {
    return authService.refreshTokens().pipe(
      switchMap(() => sendWithToken(tokenStorage.getAccessToken()!))
    );
  }

  // Reactive refresh: send request, retry once on 401
  return sendWithToken(tokenStorage.getAccessToken()!).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        return authService.refreshTokens().pipe(
          switchMap(() => sendWithToken(tokenStorage.getAccessToken()!))
        );
      }
      return throwError(() => err);
    })
  );
};
