import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { alreadyLoggedInGuard } from './already-logged-in.guard';
import { AuthService } from '../auth/auth.service';

describe('alreadyLoggedInGuard', () => {
  let mockAuthService: { isLoggedIn: ReturnType<typeof signal<boolean>> };
  let routerSpy: jasmine.SpyObj<Router>;

  const runGuard = () =>
    TestBed.runInInjectionContext(() => alreadyLoggedInGuard({} as never, {} as never));

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree']);
    routerSpy.createUrlTree.and.callFake((commands: unknown[]) => commands as unknown as UrlTree);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isLoggedIn: signal(false) } },
        { provide: Router, useValue: routerSpy }
      ]
    });

    mockAuthService = TestBed.inject(AuthService) as unknown as typeof mockAuthService;
  });

  it('should allow access to login page when NOT logged in', () => {
    mockAuthService.isLoggedIn.set(false);
    expect(runGuard()).toBeTrue();
  });

  it('should redirect to /quizzes when already logged in', () => {
    mockAuthService.isLoggedIn.set(true);
    runGuard();
    expect(routerSpy.createUrlTree).toHaveBeenCalledWith(['/quizzes']);
  });
});
