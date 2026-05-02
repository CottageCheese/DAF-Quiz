import { TestBed } from "@angular/core/testing";
import { provideHttpClient } from "@angular/common/http";
import {
  provideHttpClientTesting,
  HttpTestingController,
} from "@angular/common/http/testing";
import { Router } from "@angular/router";
import { AuthService } from "./auth.service";
import { TokenStorageService } from "./token-storage.service";
import { AuthResponse } from "../models/auth.models";

function buildFakeJwt(payload: object): string {
  const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = btoa(
    JSON.stringify({ ...payload, exp: Math.floor(Date.now() / 1000) + 900 }),
  );
  return `${header}.${body}.fake_signature`;
}

describe("AuthService", () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let mockStorage: jasmine.SpyObj<TokenStorageService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockStorage = jasmine.createSpyObj("TokenStorageService", [
      "saveTokens",
      "getAccessToken",
      "getRefreshToken",
      "isAccessTokenExpired",
      "clear",
    ]);
    routerSpy = jasmine.createSpyObj("Router", ["navigate", "createUrlTree"]);
    mockStorage.getAccessToken.and.returnValue(null);
    mockStorage.isAccessTokenExpired.and.returnValue(true);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: TokenStorageService, useValue: mockStorage },
        { provide: Router, useValue: routerSpy },
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it("should start with null user when no token in storage", () => {
    expect(service.currentUser()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });

  it("should update currentUser signal on successful login", () => {
    const fakeToken = buildFakeJwt({
      email: "user@test.com",
      display_name: "Test User",
      sub: "user-id",
    });
    const response: AuthResponse = {
      accessToken: fakeToken,
      refreshToken: "refresh",
      expiresIn: 900,
    };

    service
      .login({ email: "user@test.com", password: "Password1!" })
      .subscribe();

    const req = httpMock.expectOne((r) => r.url.includes("/api/auth/login"));
    expect(req.request.method).toBe("POST");
    req.flush(response);

    expect(service.currentUser()?.email).toBe("user@test.com");
    expect(service.currentUser()?.displayName).toBe("Test User");
    expect(service.isLoggedIn()).toBeTrue();
    expect(mockStorage.saveTokens).toHaveBeenCalledWith(response);
  });

  it("should update currentUser signal on successful register", () => {
    const fakeToken = buildFakeJwt({
      email: "new@test.com",
      display_name: "New User",
      sub: "new-id",
    });

    service
      .register({
        email: "new@test.com",
        password: "Password1!",
        displayName: "New User",
      })
      .subscribe();

    const req = httpMock.expectOne((r) => r.url.includes("/api/auth/register"));
    req.flush({ accessToken: fakeToken, refreshToken: "r", expiresIn: 900 });

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.currentUser()?.displayName).toBe("New User");
  });

  it("should clear state and navigate to login on logout", () => {
    mockStorage.getRefreshToken.and.returnValue("refresh-token");

    service.logout();

    const req = httpMock.expectOne((r) => r.url.includes("/api/auth/revoke"));
    req.flush({});

    expect(mockStorage.clear).toHaveBeenCalled();
    expect(service.currentUser()).toBeNull();
    expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
  });

  it("should deduplicate concurrent refresh calls", () => {
    mockStorage.getRefreshToken.and.returnValue("old-refresh");
    const fakeToken = buildFakeJwt({
      email: "user@test.com",
      display_name: "User",
      sub: "id",
    });

    let complete1 = false;
    let complete2 = false;

    service.refreshTokens().subscribe(() => {
      complete1 = true;
    });
    service.refreshTokens().subscribe(() => {
      complete2 = true;
    });

    // Only ONE refresh request should be made
    const requests = httpMock.match((r) => r.url.includes("/api/auth/refresh"));
    expect(requests.length).toBe(1);
    requests[0].flush({
      accessToken: fakeToken,
      refreshToken: "new-refresh",
      expiresIn: 900,
    });

    expect(complete1).toBeTrue();
    expect(complete2).toBeTrue();
  });

  it("should logout on refresh failure", () => {
    mockStorage.getRefreshToken.and.returnValue("bad-refresh");

    service.refreshTokens().subscribe({ error: () => {} });

    const refreshReq = httpMock.expectOne((r) =>
      r.url.includes("/api/auth/refresh"),
    );
    refreshReq.flush(
      { message: "Invalid token" },
      { status: 401, statusText: "Unauthorized" },
    );

    // The logout() method is called on refresh failure, which triggers a revoke request
    const revokeReq = httpMock.expectOne((r) =>
      r.url.includes("/api/auth/revoke"),
    );
    revokeReq.flush({}); // <- Flush the revoke request

    expect(mockStorage.clear).toHaveBeenCalled();
    expect(routerSpy.navigate).toHaveBeenCalledWith(["/login"]);
  });
});
