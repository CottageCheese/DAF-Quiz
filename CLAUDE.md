# DAF-Quiz

ASP.NET Core 8 quiz application with JWT authentication, admin management, and a leaderboard.

## Solution Structure

```
QuizProject.sln
├── QuizProject.Api/          # REST API (JWT Bearer, EF Core, ASP.NET Identity)
├── QuizProject.Web/          # MVC frontend (Cookie auth, calls API via IApiClient)
└── QuizProject.Tests.Integration/  # xUnit integration tests
```

## Build & Run

```bash
dotnet restore
dotnet build

# Run API (https://localhost:7001)
cd QuizProject.Api && dotnet run

# Run Web (https://localhost:5001) — separate terminal
cd QuizProject.Web && dotnet run

# Run integration tests
cd QuizProject.Tests.Integration && dotnet test
```

## Key Architectural Patterns

- **Repository pattern**: `IRepository<T>` with `Query()`, `AddAsync()`, `Update()`, `Remove()`, `SaveChangesAsync()`
- **Services**: `IQuizService`, `IAdminQuizService`, `ILeaderboardService`, `ITokenService`
- **Web has no DB access**: all data flows through `IApiClient` (HttpClient wrapper) → API
- **Auth flow**: API issues JWT (with role claims) → Web stores in cookie → `AccountController.SignInFromTokensAsync` extracts roles → `ClaimTypes.Role` on cookie principal
- **Seeded admin**: `admin@quiz.local` / `Admin123!` with `Admin` role (via `SeedData.InitialiseAsync`)

## Quiz Visibility

Quizzes have `PublishedAt` (nullable `DateTime`), **not** an `IsActive` boolean:
- Regular users only see quizzes where `PublishedAt <= now`
- Admins see all quizzes

## Adding a New Feature

New features require all layers:
1. API: service interface + implementation + controller (`[Authorize(Roles="Admin")]` for admin endpoints)
2. Web: `IApiClient` method + `SendWithAuthAsync` call + controller + views

## Admin API

Admin endpoints live at `/api/admin/quizzes` and require `Admin` role JWT.

## Database

- **Production**: SQL Server (`ConnectionStrings:DefaultConnection` in `appsettings.json`)
- **Tests**: SQLite in-memory (`CustomWebApplicationFactory`)
- **Migrations**: EF Core code-first — single `InitialCreate` migration
- **Session storage**: SQL Server distributed cache (`SessionCache` table, auto-created)

Run migrations:
```bash
cd QuizProject.Api
dotnet ef database update
```

## Ports

| Project | HTTPS | HTTP |
|---------|-------|------|
| QuizProject.Api | 7001 | 7000 |
| QuizProject.Web | 5001 | 5000 |

## Tests

Integration tests use xUnit + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing`.

```bash
# Run all tests
dotnet test

# Postman/Newman tests
./scripts/run-postman-tests.ps1 -AdminPassword "xxx" -UserPassword "xxx"
```

Test infrastructure: `CustomWebApplicationFactory`, `IntegrationTestBase`, `AuthHelper`, `TestDatabaseSeeder`.
