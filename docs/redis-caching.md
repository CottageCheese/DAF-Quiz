# Redis Caching

## Overview

The API uses `IDistributedCache` throughout. Redis is optional — if the connection string is empty the API falls back to `AddDistributedMemoryCache()` (in-process, no shared state across instances).

**Cached data:**

| Cache key | TTL | Invalidated by |
|-----------|-----|----------------|
| `quizzes:active` | 5 min | Any admin create/update/delete/publish on a quiz or question |
| `leaderboard:top-quizzes:{count}` | 12 min | Expires only (no explicit invalidation) |
| `leaderboard:top-users:{count}` | 12 min | Expires only (no explicit invalidation) |

All keys are prefixed with `QuizProject:` by StackExchange.Redis (e.g. actual Redis key = `QuizProject:quizzes:active`).

---

## Enable Redis locally

### 1. Start Redis

```bash
# Docker (easiest)
docker run -d -p 6379:6379 --name quiz-redis redis:latest

# Or if Redis is installed locally
redis-server
```

### 2. Set the connection string

Add to `QuizProject.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

> **Note:** `appsettings.json` already has the `Redis` key — it is empty by default so the app falls back to in-memory cache. Never commit a real connection string to `appsettings.json`.

### 3. Run the API

```bash
cd QuizProject.Api && dotnet run
```

Startup log will not show any Redis errors if the connection succeeded. If Redis is unreachable the app crashes at startup (StackExchange.Redis connects eagerly).

---

## Verify it is working

### Check Redis has keys

After hitting any quiz list or leaderboard endpoint:

```bash
redis-cli keys "*"
# Expected output:
# QuizProject:quizzes:active
# QuizProject:leaderboard:top-quizzes:10
# QuizProject:leaderboard:top-users:10
```

### Watch cache operations in real time

```bash
redis-cli monitor
```

- First request → `SET QuizProject:quizzes:active ...`
- Subsequent requests within TTL → `GET QuizProject:quizzes:active`
- After admin publishes/edits quiz → `DEL QuizProject:quizzes:active`

### Check via application logs

The services log cache misses at `Information`/`Debug` level:

```
Cache miss for active quizzes — querying database
Cache miss for top-quizzes (count=10) — querying database
```

No log line = cache hit.

---

## Production / Azure

Use Azure Cache for Redis. Set the connection string in environment config or Key Vault (never in `appsettings.json`):

```
ConnectionStrings__Redis=your-cache.redis.cache.windows.net:6380,password=...,ssl=True,abortConnect=False
```

The `abortConnect=False` flag is recommended for Azure so the app does not crash on transient startup connectivity issues.

---

## How the code works

`Program.cs` in `QuizProject.Api`:

```csharp
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "QuizProject:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
```

Services consume `IDistributedCache` via `DistributedCacheExtensions` (`GetAsync<T>` / `SetAsync<T>`) which serialize to/from UTF-8 JSON with camelCase naming.

Cache invalidation in `AdminQuizService` calls `cache.RemoveAsync(QuizService.ActiveQuizzesCacheKey)` after any mutating operation on quizzes or questions.
