import { TestBed } from '@angular/core/testing';
import { TokenStorageService } from './token-storage.service';
import { AuthResponse } from '../models/auth.models';

describe('TokenStorageService', () => {
  let service: TokenStorageService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenStorageService);
    localStorage.clear();
  });

  afterEach(() => localStorage.clear());

  it('should save and retrieve tokens', () => {
    const response: AuthResponse = { accessToken: 'access', refreshToken: 'refresh', expiresIn: 900 };
    service.saveTokens(response);
    expect(service.getAccessToken()).toBe('access');
    expect(service.getRefreshToken()).toBe('refresh');
  });

  it('should report token as not expired within validity window', () => {
    const response: AuthResponse = { accessToken: 'access', refreshToken: 'refresh', expiresIn: 900 };
    service.saveTokens(response);
    expect(service.isAccessTokenExpired()).toBeFalse();
  });

  it('should report expired when no tokens stored', () => {
    expect(service.isAccessTokenExpired()).toBeTrue();
  });

  it('should clear all stored tokens', () => {
    const response: AuthResponse = { accessToken: 'access', refreshToken: 'refresh', expiresIn: 900 };
    service.saveTokens(response);
    service.clear();
    expect(service.getAccessToken()).toBeNull();
    expect(service.getRefreshToken()).toBeNull();
    expect(service.isAccessTokenExpired()).toBeTrue();
  });
});
