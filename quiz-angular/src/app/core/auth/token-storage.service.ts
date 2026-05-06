import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly ACCESS_KEY = 'quiz_access_token';
  private readonly REFRESH_KEY = 'quiz_refresh_token';
  private readonly EXPIRY_KEY = 'quiz_token_expiry';

  saveTokens(response: AuthResponse): void {
    const expiryMs = Date.now() + response.expiresIn * 1000;
    localStorage.setItem(this.ACCESS_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_KEY, response.refreshToken);
    localStorage.setItem(this.EXPIRY_KEY, String(expiryMs));
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  isAccessTokenExpired(): boolean {
    const expiry = localStorage.getItem(this.EXPIRY_KEY);
    if (!expiry) return true;
    return Date.now() >= Number(expiry) - 30_000; // 30s buffer
  }

  clear(): void {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.EXPIRY_KEY);
  }
}
