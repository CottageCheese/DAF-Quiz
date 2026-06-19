---
name: daf-security
description: >
  Security audit for DAF-Quiz (ASP.NET Core 8, JWT, EF Core MVC).
  Checks JWT config, authorization attributes, cookie security, input validation, secrets,
  and HTTPS setup. Use when: "security check", "audit auth", "/daf-security", or before
  merging to main when auth/API changes are involved.
disable-model-invocation: true
context: fork
agent: Explore
---

Security audit for DAF-Quiz. Scan $ARGUMENTS (if empty: full solution).

## Current State

- Branch: !`git branch --show-current`
- Changed files: !`git diff --name-only main...HEAD`

## Output Format

One line per finding: `<file>:L<line>: [SEVERITY] <problem>. <fix>.`

Severity: `🔴 critical` | `🟡 risk` | `🔵 nit`

Lead with summary: `Found X critical, Y risk, Z nit.`

For `🔴 critical` findings: write full paragraph explaining the vulnerability and exact fix.
For `🟡`/`🔵`: one line only.

If category clean: `✅ <Category>: clean`

---

## Checklist

### JWT / Authentication
- [ ] `JwtSecretKey` not hardcoded in source — must come from env var or user-secrets
- [ ] Key length >= 32 chars (256-bit minimum for HMAC-SHA256)
- [ ] `ValidateIssuerSigningKey`, `ValidateIssuer`, `ValidateAudience` all `true`
- [ ] Token expiry configured and reasonable (not days/weeks)
- [ ] No JWT tokens written to logs or returned in error bodies

### Authorization
- [ ] All admin controllers/actions have `[Authorize(Roles="Admin")]`
- [ ] All non-public API endpoints have `[Authorize]`
- [ ] No sensitive data reachable by unauthenticated callers
- [ ] `PublishedAt` filter enforced — `null` quizzes never exposed to non-admins

### Cookie Security
- [ ] Auth cookies: `HttpOnly = true`, `Secure = true`, `SameSite = Strict`
- [ ] Public site cookie name `.QuizProject.Auth` — admin `.QuizProject.Admin.Auth` (must be isolated)
- [ ] Session cookies contain no sensitive data (tokens stored via `ITokenStorageService` only)

### Input Validation
- [ ] `[ApiController]` present — automatic model validation active
- [ ] No raw SQL strings — EF Core parameterized queries only
- [ ] No user input interpolated into `FromSqlRaw` / `ExecuteSqlRaw`
- [ ] Passwords never logged or included in API responses

### Secrets Management
- [ ] No connection strings or JWT keys in committed `appsettings.json`
- [ ] `appsettings.Development.json` excluded from source control (`.gitignore`)
- [ ] `dotnet user-secrets` or env vars used for local dev secrets

### HTTPS / Transport
- [ ] `app.UseHsts()` in production pipeline
- [ ] `app.UseHttpsRedirection()` present
- [ ] CORS not configured as `AllowAnyOrigin` + `AllowCredentials` (browser blocks this + security risk)
