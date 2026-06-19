---
name: daf-scale
description: >
  Scalability and performance audit for DAF-Quiz (EF Core 8, async ASP.NET Core).
  Checks N+1 queries, missing includes, async/await misuse, unbounded list loads, and
  resource disposal. Use when: "perf check", "query audit", "/daf-scale", or when
  adding/modifying EF Core queries or service methods.
disable-model-invocation: true
context: fork
agent: Explore
---

Scalability audit for DAF-Quiz. Scan $ARGUMENTS (if empty: Domain + API layers).

## Current State

- Branch: !`git branch --show-current`
- Changed files: !`git diff --name-only main...HEAD`

## Output Format

One line per finding: `<file>:L<line>: [SEVERITY] <problem>. <fix>.`

Severity: `🔴 critical` | `🟡 risk` | `🔵 nit`

Lead with summary: `Found X critical, Y risk, Z nit.`

If category clean: `✅ <Category>: clean`

---

## Checklist

### EF Core / Queries
- [ ] No N+1: related entities loaded via `.Include()` / `.ThenInclude()` not in loops
- [ ] No `.ToList()` / `.ToArray()` before `.Where()` / `.Select()` — filter in DB
- [ ] Large list endpoints use pagination (`Skip`/`Take`) — no unbounded `GetAll()`
- [ ] Queries project to DTOs via `.Select(x => new Dto {...})` not load full entities when only subset needed
- [ ] `ApplicationDbContext` not injected as singleton or stored in static field
- [ ] `SaveChangesAsync()` used not `SaveChanges()`
- [ ] No `context.Database.ExecuteSqlRaw` in hot paths without measuring

### Async Patterns
- [ ] No `.Result` or `.Wait()` on `Task` — deadlocks ASP.NET Core sync context
- [ ] No `.GetAwaiter().GetResult()` outside of legitimate sync entry points
- [ ] All controller actions `async Task<IActionResult>` (or `async Task<ActionResult<T>>`)
- [ ] No `async void` except event handlers
- [ ] `CancellationToken` parameter threaded through service calls where available

### Resource Management
- [ ] `IDisposable` resources in `using` blocks or disposed via DI lifetime
- [ ] No manual `new ApplicationDbContext(...)` outside of tests — use DI
- [ ] `HttpClient` not instantiated with `new` in services — use `IHttpClientFactory`

### Caching
- [ ] Leaderboard / static data uses `IDistributedCache` (SQL Server-backed) not recomputed per request
- [ ] Cache keys include discriminating params (user id, quiz id) to avoid cross-user pollution
