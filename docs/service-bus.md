# Azure Service Bus

## Overview

When a quiz-taker submits an attempt the API publishes a `QuizAttemptCompletedEvent` to an Azure Service Bus topic. Two independent subscribers process the event asynchronously — the HTTP response is returned to the user before either subscriber runs.

**Event flow:**

```
POST /api/quizzes/attempts/{id}/submit
    → score saved to database
    → QuizAttemptCompletedEvent published to topic: quiz-events
    → HTTP 200 returned to caller

    [background — decoupled from HTTP request]
    ├── leaderboard-invalidation subscription
    │       removes leaderboard cache keys so next read is fresh
    └── quiz-result-notification subscription
            logs result (stub — replace with email/SignalR/push)
```

**Effect on caching** (extends `docs/redis-caching.md`):

| Cache key | TTL | Invalidated by |
|-----------|-----|----------------|
| `quizzes:active` | 5 min | Admin create/update/delete/publish |
| `leaderboard:top-quizzes:{count}` | 12 min | **Service Bus** — on every quiz submission |
| `leaderboard:top-users:{count}` | 12 min | **Service Bus** — on every quiz submission |

Without Service Bus the leaderboard keys expired only after 12 minutes. With it, rankings update immediately after each submission.

**When Service Bus is disabled** (empty `ConnectionStrings:ServiceBus`): a no-op `NullQuizEventPublisher` is registered. The app runs normally; leaderboard falls back to TTL-only invalidation. No SDK is loaded, no background threads start.

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) — required for the local emulator

---

## Local Setup — Azure Service Bus Emulator

Microsoft ships an official emulator that uses the same SDK and connection string format as production Azure.

### 1. Start the emulator

Run from the **repo root** — the config file is mounted to pre-create the topic and subscriptions:

```bash
docker run -d --name sb-emulator \
  -p 5672:5672 \
  -v "$(pwd)/docs/service-bus-emulator-config.json:/ServiceBus_Emulator/ConfigFiles/Config.json" \
  mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
```

This creates:
- Topic: `quiz-events`
- Subscription: `leaderboard-invalidation`
- Subscription: `quiz-result-notification`

### 2. Set the connection string

```bash
dotnet user-secrets set "ConnectionStrings:ServiceBus" \
  "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;" \
  --project QuizProject.Api
```

> `appsettings.json` already has `"ServiceBus": ""` as a placeholder. Never commit a real connection string there.

### 3. Run the API

```bash
cd QuizProject.Api && dotnet run
```

Startup log will include:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
```

No errors = emulator connected. If the emulator is unreachable the `ServiceBusClient` will surface errors when it first tries to send (not at startup).

---

## Verify It Is Working

Submit a quiz attempt (via the MVC app or Swagger at `https://localhost:7001/swagger`). Within a second the API logs should show:

```
info: QuizProject.Api.Messaging.ServiceBusQuizEventPublisher[0]
      Published QuizAttemptCompletedEvent for attempt 42

info: QuizProject.Api.Messaging.LeaderboardInvalidationConsumer[0]
      Leaderboard cache invalidated after attempt 42 (quiz 1)

info: QuizProject.Api.Messaging.QuizResultNotificationConsumer[0]
      RESULT NOTIFICATION [stub] — 'Alice' completed 'History Quiz': 8/10 (80.0%)
```

Then call `GET /api/leaderboard/top-users` — the response reflects the new score immediately rather than waiting up to 12 minutes for the cache TTL.

**If publish fails** (emulator stopped mid-run): the API logs an `Error` but the quiz result is still returned to the caller. The score is already committed to the database.

---

## Production / Azure

Use an Azure Service Bus **Standard tier** namespace (Basic tier does not support topics/subscriptions).

Create the required resources:

```bash
az servicebus namespace create --name daf-quiz-sb --resource-group <rg> --sku Standard
az servicebus topic create --name quiz-events --namespace-name daf-quiz-sb --resource-group <rg>
az servicebus topic subscription create --name leaderboard-invalidation --topic-name quiz-events --namespace-name daf-quiz-sb --resource-group <rg>
az servicebus topic subscription create --name quiz-result-notification --topic-name quiz-events --namespace-name daf-quiz-sb --resource-group <rg>
```

Set the connection string in Azure App Service configuration or Key Vault (never in `appsettings.json`):

```
ConnectionStrings__ServiceBus=Endpoint=sb://daf-quiz-sb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=...
```

No code change is required — the same `ServiceBusQuizEventPublisher` and both consumers run against real Azure.

---

## How the Code Works

| File | Role |
|------|------|
| `QuizProject.Contracts/IQuizEventPublisher.cs` | Interface — lives in Contracts so `QuizService` (Domain) can depend on it without referencing Api |
| `QuizProject.Contracts/QuizAttemptCompletedEvent.cs` | Message contract — a flat record with all data consumers need |
| `QuizProject.Contracts/NullQuizEventPublisher.cs` | No-op registered when connection string is empty (CI, tests) |
| `QuizProject.Api/Messaging/ServiceBusQuizEventPublisher.cs` | Serialises the event to JSON and sends to the `quiz-events` topic. Sets `MessageId = "attempt-{id}"` for idempotency |
| `QuizProject.Api/Messaging/LeaderboardInvalidationConsumer.cs` | `BackgroundService` — reads `leaderboard-invalidation` subscription, removes cache keys |
| `QuizProject.Api/Messaging/QuizResultNotificationConsumer.cs` | `BackgroundService` — reads `quiz-result-notification` subscription, logs result stub |

`Program.cs` registration (after the Redis block):

```csharp
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");
if (!string.IsNullOrWhiteSpace(serviceBusConnection))
{
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnection));
    builder.Services.AddSingleton<IQuizEventPublisher, ServiceBusQuizEventPublisher>();
    builder.Services.AddHostedService<LeaderboardInvalidationConsumer>();
    builder.Services.AddHostedService<QuizResultNotificationConsumer>();
}
else
{
    builder.Services.AddSingleton<IQuizEventPublisher, NullQuizEventPublisher>();
}
```

`ServiceBusClient` is Singleton — the SDK is thread-safe and manages connection pooling internally.
