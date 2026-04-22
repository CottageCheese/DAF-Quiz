# DAF-Quiz — Claude Instructions

## What the Solution Does

DAF-Quiz is an ASP.NET Core 8 quiz platform with two projects:

- **`QuizProject.Api`** — JWT REST API; the authoritative backend consumed by all clients
- **`QuizProject.Web`** — MVC Razor frontend; a thin client over the API (may be supplemented or replaced by Angular)

**User flows:** register/login → browse published quizzes → take a quiz → view score and leaderboard
**Admin flows:** create/edit/delete quizzes, manage questions and answers, control visibility via `PublishedAt`

The API is designed for external consumers (Angular SPA, mobile, third-party). All business logic lives in the API; no client should bypass it.

---

## Project Layout

```
DAF-Quiz/
├── QuizProject.Api/
│   ├── Controllers/          # Thin; delegate all logic to services
│   ├── Data/                 # ApplicationDbContext, SeedData (admin seed)
│   ├── Models/Domain/        # EF Core entities: ApplicationUser, Quiz, Question, Answer,
│   │                         #   QuizAttempt, QuizAttemptAnswer, RefreshToken
│   ├── Models/ViewModels/    # Request/response DTOs — never expose domain entities directly
│   ├── Repositories/         # IRepository<T> / Repository<T> generic abstraction
│   ├── Services/             # ITokenService, IQuizService, IAdminQuizService, ILeaderboardService
│   ├── Migrations/           # EF Core code-first migrations (InitialCreate)
│   └── Program.cs            # Composition root
│
├── QuizProject.Web/
│   ├── Controllers/          # Thin; call IApiClient, map to view models
│   ├── Models/ViewModels/    # View/form-specific DTOs
│   ├── Services/             # IApiClient (HTTP wrapper), ITokenStorageService (session)
│   ├── Views/                # Razor views by feature (Account, Quiz, Admin, Home)
│   └── Program.cs
│
└── CLAUDE.md
```

Future Angular project: add as a peer directory (e.g. `quiz-web-angular/`), consuming the API directly over HTTP.

---

## Architecture Guidelines

- **Layered / clean architecture:** outer layers depend on inner layers via interfaces; domain entities have no infrastructure dependencies
- **Thin controllers:** bind input → call service → return result; no business logic in controllers
- **Services own business logic;** repositories own data access
- **DTOs cross layer boundaries;** domain entities must not reach views or API responses
- Use `IRepository<T>` for generic CRUD; use `Query()` (IQueryable) for complex read queries to avoid N+1
- Cross-cutting concerns (auth, logging, exception handling, rate limiting, security headers) are centralised in middleware/filters, not scattered in controllers or services

---

## API Design

- Routes: `/api/{resource}` and `/api/admin/{resource}`
- Error responses use `ProblemDetails` (RFC 7807) — do not return raw exceptions
- Auth: JWT Bearer; refresh token rotation with reuse detection (token hashed in DB as SHA-256)
- CORS: configured via `AllowedOrigins` in `appsettings.json` — adding a new client only requires updating this list
- Rate limiting: fixed window, 10 req/min on auth endpoints
- New endpoints must be Swagger/OpenAPI-documentable; no undocumented side effects
- The API must remain frontend-agnostic — never couple API logic to a specific client

---

## Dependency Injection Lifetimes

| Registration | Lifetime | Reason |
|---|---|---|
| `ApplicationDbContext` | Scoped | Per-request; EF Core requirement |
| `Repository<T>` | Scoped | Wraps DbContext |
| `TokenService`, `QuizService`, `AdminQuizService`, `LeaderboardService` | Scoped | Stateless per-request |
| `IMemoryCache` | Singleton | Thread-safe, long-lived |
| `SemaphoreSlim` (token refresh lock in `ApiClient`) | Singleton | Prevents concurrent refresh races |

---

## Security Rules

- **Never commit secrets.** Use `dotnet user-secrets` in dev; environment variables or Azure Key Vault in production
- `JwtSettings.SecretKey` must be ≥ 32 characters; validate at startup
- Always verify user ownership on mutations: `attempt.UserId == currentUserId`
- Refresh token raw values are hashed (SHA-256) before storage; raw tokens must never be persisted
- Cookie auth (Web): HttpOnly, Secure, SameSite=Strict, 2-hour sliding expiry
- Anti-forgery tokens required on all POST/PUT/DELETE forms (Web)
- Account lockout: 5 failed attempts → 5-minute lockout (built into Identity config)

---

## Coding Conventions

- C# 12 / .NET 8; nullable reference types enabled — prefer non-nullable where possible
- 4-space indent; ~120-character line width
- **Naming:** PascalCase for types/methods/properties; camelCase for locals and parameters; `I` prefix on interfaces
- Throw specific exceptions; unhandled exceptions caught at middleware boundary with user-friendly responses
- `async/await` on all I/O; never use `.Result` or `.Wait()`
- XML docs on public API surface; prefer self-explanatory code; no commented-out code in commits
- Prefer composition over inheritance; keep classes small and focused

---

## Testing

- **Unit tests:** domain logic and application services in isolation; mock `IRepository<T>` and external services
- **Integration tests:** `WebApplicationFactory` + in-memory SQLite for HTTP pipeline tests
- Test project: `QuizProject.Tests/` (to be created alongside the solution)
- Tests run in CI on every PR; no flaky or environment-dependent tests committed
- Include tests for any behavior change; pure refactors require a passing baseline

---

## Adding New Features — Checklist

1. Add/update domain entity in `QuizProject.Api/Models/Domain/`
2. Add EF Core migration: `dotnet ef migrations add <MigrationName>`
3. Add a specific repository interface/implementation if the generic `IRepository<T>` is insufficient
4. Implement the service method behind its interface
5. Register the service in `QuizProject.Api/Program.cs`
6. Add the controller endpoint (API) — thin, delegates to the service
7. Add request/response DTOs in `QuizProject.Api/Models/ViewModels/`
8. Update `IApiClient` and `ApiClient` in `QuizProject.Web/Services/` if the Web project needs it
9. Add/update Razor views (Web) **and/or** document the new endpoint for the Angular team
10. Write unit and integration tests

---

## Future: Angular Frontend

- Will be a standalone SPA consuming `QuizProject.Api` over HTTP
- Only change needed on the API side: add the Angular origin to `AllowedOrigins` in `appsettings.json`
- **Token storage:** store JWT/refresh tokens in memory (Angular service), not `localStorage` or `sessionStorage`, to mitigate XSS token theft
- **Auth flow:** POST `/api/auth/login` → receive `accessToken` + `refreshToken` → attach Bearer header → intercept 401 → call `/api/auth/refresh` → retry
- The MVC Web project may continue as an admin-only interface or be retired once Angular covers all features; do not remove the API endpoints that Web currently uses until Angular has full parity
