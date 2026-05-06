import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { TokenStorageService } from '../auth/token-storage.service';
import { AuthService } from '../auth/auth.service';

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let http: HttpClient;
  let mockStorage: jasmine.SpyObj<TokenStorageService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    mockStorage = jasmine.createSpyObj('TokenStorageService', [
      'getAccessToken', 'isAccessTokenExpired', 'getRefreshToken', 'saveTokens', 'clear'
    ]);
    mockAuthService = jasmine.createSpyObj('AuthService', ['refreshTokens', 'logout']);
    mockStorage.isAccessTokenExpired.and.returnValue(false);
    mockStorage.getAccessToken.and.returnValue('valid_token');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: TokenStorageService, useValue: mockStorage },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate', 'createUrlTree']) }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    http = TestBed.inject(HttpClient);
  });

  afterEach(() => httpMock.verify());

  it('should attach Authorization header to API requests', () => {
    http.get('/api/quizzes').subscribe();
    const req = httpMock.expectOne('/api/quizzes');
    expect(req.request.headers.get('Authorization')).toBe('Bearer valid_token');
    req.flush([]);
  });

  it('should NOT attach Authorization header to /api/auth/ endpoints', () => {
    http.post('/api/auth/login', {}).subscribe();
    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('should NOT attach Authorization header to /api/auth/register', () => {
    http.post('/api/auth/register', {}).subscribe();
    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });
});
