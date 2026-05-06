# DAF-Quiz

ASP.NET Core 8 quiz application with JWT authentication, admin management, and a leaderboard.

## Solution Structure

```
QuizProject.sln
├── QuizProject.Contracts/        # Pure DTOs/ViewModels — no dependencies
├── QuizProject.Domain/           # Entities, EF Core, repositories, domain services
├── QuizProject.Api/              # REST API (JWT Bearer, ASP.NET Identity)
├── QuizProject.Web/              # MVC frontend (Cookie auth, calls API via IApiClient)
├── QuizProject.Domain.Tests/     # xUnit unit tests for domain services (SQLite in-memory)
└── QuizProject.Tests.Integration/  # xUnit integration tests (HTTP surface)
```

## Project Reference Graph

```
QuizProject.Contracts       (no deps)
       ↑
QuizProject.Domain          (+ EF Core, Identity)
       ↑                ↑
QuizProject.Api    QuizProject.Domain.Tests
       ↑
QuizProject.Web (→ Contracts only, no Domain)
       ↑
QuizProject.Tests.Integration
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

- **Repository pattern**: `IRepository<T>` with `Query()`, `AddAsync()`, `Update()`, `Remove()`, `SaveChangesAsync()` — lives in `QuizProject.Domain`
- **Services**: `IQuizService`, `IAdminQuizService`, `ILeaderboardService` in `QuizProject.Domain.Services`; `ITokenService` stays in `QuizProject.Api.Services` (JWT infrastructure, not domain logic)
- **Contracts**: all shared DTOs/ViewModels live in `QuizProject.Contracts` (flat namespace, no sub-namespaces)
- **Web has no DB access**: all data flows through `IApiClient` (HttpClient wrapper) → API
- **Auth flow**: API issues JWT (with role claims) → Web stores in cookie → `AccountController.SignInFromTokensAsync` extracts roles → `ClaimTypes.Role` on cookie principal
- **Seeded admin**: `admin@quiz.local` / `Admin123!` with `Admin` role (via `SeedData.InitialiseAsync`)

## Quiz Visibility

Quizzes have `PublishedAt` (nullable `DateTime`), **not** an `IsActive` boolean:
- Regular users only see quizzes where `PublishedAt <= now`
- Admins see all quizzes

## Adding a New Feature

New features require all layers:
1. **Contracts**: add ViewModel/request types to `QuizProject.Contracts` (flat namespace `QuizProject.Contracts`)
2. **Domain**: add service interface + implementation to `QuizProject.Domain.Services`
3. **API**: add controller + register service in `Program.cs` (`[Authorize(Roles="Admin")]` for admin endpoints)
4. **Web**: add `IApiClient` method + `SendWithAuthAsync` call + controller + views

## Admin API

Admin endpoints live at `/api/admin/quizzes` and require `Admin` role JWT.

## Database

- **Production**: SQL Server (`ConnectionStrings:DefaultConnection` in `appsettings.json`)
- **Tests**: SQLite in-memory (`CustomWebApplicationFactory`)
- **Migrations**: EF Core code-first — single `InitialCreate` migration
- **Session storage**: SQL Server distributed cache (`SessionCache` table, auto-created)

Migrations live in `QuizProject.Domain`. Run via:
```bash
dotnet ef migrations add <Name> --project QuizProject.Domain --startup-project QuizProject.Api
dotnet ef database update --project QuizProject.Domain --startup-project QuizProject.Api
```

## Ports

| Project | HTTPS | HTTP |
|---------|-------|------|
| QuizProject.Api | 7001 | 7000 |
| QuizProject.Web | 5001 | 5000 |

## Tests

Two test suites:

**Domain unit tests** (`QuizProject.Domain.Tests`) — SQLite in-memory, no HTTP:
- `DomainTestBase`: wires repositories + `MemoryCache` directly; `IDisposable`
- `DomainTestSeeder`: seeds entities via `ApplicationDbContext` directly (no `UserManager`)

**Integration tests** (`QuizProject.Tests.Integration`) — full HTTP stack via `WebApplicationFactory`:
- `CustomWebApplicationFactory`, `IntegrationTestBase`, `AuthHelper`, `TestDatabaseSeeder`

```bash
# Run all tests
dotnet test QuizProject.sln

# Domain tests only
dotnet test QuizProject.Domain.Tests

# Integration tests only
dotnet test QuizProject.Tests.Integration

# Postman/Newman tests
./scripts/run-postman-tests.ps1 -AdminPassword "xxx" -UserPassword "xxx"
```
