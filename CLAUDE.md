# DAF-Quiz

ASP.NET Core 8 quiz application with JWT authentication, admin management, and a leaderboard.

## Solution Structure

```
QuizProject.sln
├── QuizProject.Contracts/        # Pure DTOs/ViewModels — no dependencies
├── QuizProject.Domain/           # Entities, EF Core, domain services
├── QuizProject.Api/              # REST API (JWT Bearer, ASP.NET Identity)
├── QuizProject.Web.Common/       # Shared web infrastructure (session, cookie auth, ApiClientBase)
├── QuizProject.Web/              # Public MVC site (leaderboard, quiz-taking, registration)
├── QuizProject.Web.Admin/        # Admin MVC site (quiz/question CRUD, admin-only)
├── QuizProject.Domain.Tests/     # xUnit unit tests for domain services (SQLite in-memory)
└── QuizProject.Tests.Integration/  # xUnit integration tests (HTTP surface)
```

## Project Reference Graph

```
QuizProject.Contracts       (no deps)
       ↑          ↑
QuizProject.Domain    QuizProject.Web.Common (+ JWT, SqlServer cache)
       ↑  ↑                  ↑              ↑
QuizProject.Api  QuizProject.Domain.Tests  QuizProject.Web  QuizProject.Web.Admin
       ↑
QuizProject.Tests.Integration
```

## Build & Run

```bash
dotnet restore
dotnet build

# Run API (https://localhost:7001)
cd QuizProject.Api && dotnet run

# Run Public Web (https://localhost:5001) — separate terminal
cd QuizProject.Web && dotnet run

# Run Admin Web (https://localhost:5003) — separate terminal
cd QuizProject.Web.Admin && dotnet run

# Run integration tests
cd QuizProject.Tests.Integration && dotnet test
```

## Key Architectural Patterns

> **AVOID the Repository pattern.** Do not introduce `IRepository<T>` or any repository abstraction. Services use `ApplicationDbContext` (EF Core) directly.

- **Services**: `IQuizService`, `IAdminQuizService`, `ILeaderboardService` in `QuizProject.Domain.Services`; `ITokenService` stays in `QuizProject.Api.Services` (JWT infrastructure, not domain logic)
- **Contracts**: all shared DTOs/ViewModels live in `QuizProject.Contracts` (flat namespace, no sub-namespaces)
- **Web has no DB access**: all data flows through `IPublicApiClient` / `IAdminApiClient` (HttpClient wrappers) → API
- **Shared web infra**: `QuizProject.Web.Common` holds `ApiClientBase`, `AccountControllerBase`, `ITokenStorageService`, `WebCommonServiceExtensions` — consumed by both Web projects
- **Auth flow**: API issues JWT (with role claims) → Web stores in cookie → `AccountControllerBase.SignInFromTokensAsync` extracts roles → `ClaimTypes.Role` on cookie principal
- **Cookie isolation**: Public site uses `.QuizProject.Session`/`.QuizProject.Auth`; Admin uses `.QuizProject.Admin.Session`/`.QuizProject.Admin.Auth`
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
4. **Web (public)**: add `IPublicApiClient` method + `SendWithAuthAsync` call + controller + views in `QuizProject.Web`
5. **Web (admin)**: add `IAdminApiClient` method + controller + views in `QuizProject.Web.Admin`

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
| QuizProject.Web (Public) | 5001 | 5000 |
| QuizProject.Web.Admin | 5003 | 5002 |

## Tests

Two test suites:

**Domain unit tests** (`QuizProject.Domain.Tests`) — SQLite in-memory, no HTTP:
- `DomainTestBase`: wires services + `MemoryCache` directly; `IDisposable`
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
