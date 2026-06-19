---
name: daf-conventions
description: >
  Background architecture rules for DAF-Quiz. Apply automatically when writing or reviewing
  any code in this solution — no user invocation needed. Enforces layer boundaries, naming,
  DI lifetimes, and project-specific patterns from CLAUDE.md.
user-invocable: false
---

## Layer Boundaries — ENFORCE ALWAYS

**Web projects (`QuizProject.Web`, `QuizProject.Web.Admin`) MUST NOT:**
- Reference `ApplicationDbContext` directly
- Reference `QuizProject.Domain` services directly
- Access any EF Core types

**All Web data access goes through:**
- `IPublicApiClient` (public site) → calls API via HTTP
- `IAdminApiClient` (admin site) → calls API via HTTP with JWT

**API project (`QuizProject.Api`) owns:**
- JWT issuance (`ITokenService` stays here — not moved to Domain)
- `[Authorize]` / `[Authorize(Roles="Admin")]` enforcement

**Domain project (`QuizProject.Domain`) owns:**
- `ApplicationDbContext`, EF Core entities, migrations
- `IQuizService`, `IAdminQuizService`, `ILeaderboardService` implementations

## Forbidden Patterns

- **No repository pattern** — no `IRepository<T>`, no `IQuizRepository`. Services use `ApplicationDbContext` directly.
- **No domain entities in API responses** — always map to contracts (`QuizProject.Contracts`) before returning
- **No sub-namespaces in Contracts** — all DTOs/ViewModels in flat `QuizProject.Contracts` namespace

## Naming & Structure

- Contracts: flat namespace `QuizProject.Contracts`, suffix `Dto` for API types, `ViewModel` for MVC views
- Services: interface `IXxxService` in Domain, implementation `XxxService` — register as scoped
- Controllers: thin — delegate to services, no business logic
- New features need all layers: Contracts → Domain service → API controller → Web client method → Web controller + views

## Quiz Visibility Rule

```csharp
// Regular users: PublishedAt <= DateTime.UtcNow
// Admins: no filter
```
Never use `IsActive` boolean — `PublishedAt` nullable datetime is the single source of truth.

## Auth Cookie Names — Must Not Change

| Site | Session cookie | Auth cookie |
|------|---------------|-------------|
| Public (`QuizProject.Web`) | `.QuizProject.Session` | `.QuizProject.Auth` |
| Admin (`QuizProject.Web.Admin`) | `.QuizProject.Admin.Session` | `.QuizProject.Admin.Auth` |

Changing these breaks cross-site isolation.

## DI Lifetimes

- EF Core-dependent services → **Scoped**
- `ITokenStorageService` → **Scoped** (per-request cookie/session access)
- `IHttpClientFactory`-based clients → **Transient** or factory-managed
- Never register EF-dependent service as **Singleton**

## Seeded Admin

Default admin: `admin@quiz.local` / `Admin123!` with `Admin` role via `SeedData.InitialiseAsync`.
Do not hardcode these credentials anywhere except `SeedData` — use environment config in production.
