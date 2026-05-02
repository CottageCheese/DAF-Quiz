import { TestBed } from "@angular/core/testing";
import { Router, UrlTree } from "@angular/router";
import { signal } from "@angular/core";
import { authGuard } from "./auth.guard";
import { AuthService } from "../auth/auth.service";

describe("authGuard", () => {
  let mockAuthService: { isLoggedIn: ReturnType<typeof signal<boolean>> };
  let routerSpy: jasmine.SpyObj<Router>;

  const runGuard = () =>
    TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));

  beforeEach(() => {
    routerSpy = jasmine.createSpyObj("Router", ["navigate", "createUrlTree"]);
    routerSpy.createUrlTree.and.callFake(
      (commands: unknown[]) => commands as unknown as UrlTree,
    );

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isLoggedIn: signal(false) } },
        { provide: Router, useValue: routerSpy },
      ],
    });

    mockAuthService = TestBed.inject(
      AuthService,
    ) as unknown as typeof mockAuthService;
  });

  it("should allow access when logged in", () => {
    mockAuthService.isLoggedIn.set(true);
    expect(runGuard()).toBeTrue();
  });

  it("should redirect to /login when not logged in", () => {
    mockAuthService.isLoggedIn.set(false);
    runGuard(); // <- Add this line to actually execute the guard
    expect(routerSpy.createUrlTree).toHaveBeenCalledWith(["/login"]);
  });
});
