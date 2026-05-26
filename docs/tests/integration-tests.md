# Integration Tests

Project: `QuizProject.Tests.Integration`

Full HTTP stack via `WebApplicationFactory<Program>`. Tests hit real endpoints with real middleware, auth, routing, and validation — only the database is swapped (SQL Server → SQLite in-memory).

## Run

```bash
# All integration tests
dotnet test QuizProject.Tests.Integration

# Single collection
dotnet test QuizProject.Tests.Integration --filter "Category=Auth"

# Single test
dotnet test QuizProject.Tests.Integration --filter "FullyQualifiedName~Login_ValidCredentials"
```

## Infrastructure

### CustomWebApplicationFactory

Replaces production wiring for the test environment:

- **Database**: SQL Server → SQLite in-memory (unique per factory instance, GUID in connection string)
- **Config**: loads `appsettings.Test.json` overrides
- **Seed**: skips `SeedData.InitialiseAsync` (production seeder); runs `TestDatabaseSeeder` instead
- **Rate limiting**: disabled
- **Environment**: `"Testing"`

Implements `IAsyncLifetime`. Database is created and seeded in `InitializeAsync()`.

### IntegrationTestBase

Abstract base all test classes extend. Uses `IClassFixture<CustomWebApplicationFactory>`.

```csharp
protected CustomWebApplicationFactory Factory { get; }
protected HttpClient Client { get; }          // unauthenticated
protected TestSeedContext Seed { get; }
```

Authentication helpers:

```csharp
await CreateAdminClientAsync()   // HttpClient with admin Bearer token
await CreateUserClientAsync()    // HttpClient with user Bearer token
await GetAdminTokenAsync()       // raw JWT string
await GetUserTokenAsync()        // raw JWT string
```

### TestDatabaseSeeder

Seeded credentials used across all integration tests:

| Account | Email | Password |
|---------|-------|----------|
| Admin | `admin@test.local` | `Admin@123Test!` |
| User | `user@test.local` | `User@123Test!` |

Seeded data:

| Entity | Details |
|--------|---------|
| Published quiz | 3 questions, 4 answers each (1 correct, 3 wrong) |
| Draft quiz | No `PublishedAt` |
| Pre-completed attempt | User has a 3/3 attempt on the published quiz |

All IDs and credentials are exposed via `TestSeedContext`.

### AuthHelper

Calls `POST /api/auth/login` to get real tokens. Caches `AuthResponse` per `(email, password)` to avoid repeated roundtrips within the same factory lifetime.

### Test Collections

Four xUnit collection fixtures isolate factory instances per domain:

| Collection | Factory |
|------------|---------|
| `AuthTestsCollection` | Shared across Auth tests |
| `QuizTestsCollection` | Shared across Quiz tests |
| `AdminTestsCollection` | Shared across Admin tests |
| `LeaderboardTestsCollection` | Shared across Leaderboard tests |

Each collection gets its own `CustomWebApplicationFactory` and seeded database.

---

## Test Classes

### Auth/RegisterTests (6 methods)

`POST /api/auth/register`

| Test | Expected |
|------|----------|
| `Register_ValidRequest_Returns200WithTokens` | 200, tokens in body |
| `Register_DuplicateEmail_Returns409` | 409 Conflict |
| `Register_DuplicateDisplayName_Returns409` | 409 Conflict |
| `Register_WeakPassword_Returns400` | 400 validation error |
| `Register_MissingEmail_Returns400` | 400 validation error |
| `Register_MissingDisplayName_Returns400` | 400 validation error |

---

### Auth/LoginTests (5 methods)

`POST /api/auth/login`

| Test | Expected |
|------|----------|
| `Login_ValidCredentials_Returns200WithTokens` | 200, `expiresIn = 900` (15 min) |
| `Login_WrongPassword_Returns401` | 401 |
| `Login_NonExistentEmail_Returns401` | 401 |
| `Login_EmptyBody_Returns400` | 400 validation error |
| `Login_AccountLockout_Returns429After5Failures` | 429 after 5 bad attempts |

---

### Auth/RefreshTests & Auth/RevokeTests

`POST /api/auth/refresh` and `POST /api/auth/revoke`

Cover token rotation, replay detection, and explicit revocation.

---

### Quizzes/GetQuizzesTests (4 methods)

`GET /api/quizzes`

| Test | Expected |
|------|----------|
| `GetQuizzes_Authenticated_Returns200WithList` | 200, published quizzes only |
| `GetQuizzes_DoesNotReturnDraftQuiz` | Draft quiz absent from response |
| `GetQuizzes_NoAuth_Returns401` | 401 |
| `GetQuizzes_ResponseShape_HasAllRequiredFields` | `Title`, `QuestionCount`, `CreatedAt` present |

---

### Quizzes/StartAttemptTests (5 methods)

`POST /api/quizzes/{id}/start`

| Test | Expected |
|------|----------|
| `StartAttempt_PublishedQuiz_Returns200WithViewModel` | 200, attempt returned |
| `StartAttempt_EachQuestionHasAnswers_IsCorrectNotExposed` | `isCorrect` absent from JSON |
| `StartAttempt_DraftQuiz_Returns404` | 404 |
| `StartAttempt_NonExistentQuiz_Returns404` | 404 |
| `StartAttempt_NoAuth_Returns401` | 401 |

---

### Quizzes/SubmitAttemptTests & Quizzes/GetResultTests

`POST /api/quizzes/{id}/submit` and `GET /api/quizzes/{id}/result`

Cover score calculation, grade labels, and result access authorization.

---

### Admin/AdminQuizCrudTests (14 methods)

`/api/admin/quizzes` — requires `Admin` role.

| Test | Expected |
|------|----------|
| `GetAllQuizzes_Admin_Returns200WithDraftsAndPublished` | Admin sees draft + published |
| `GetAllQuizzes_RegularUser_Returns403` | 403 Forbidden |
| `GetAllQuizzes_NoAuth_Returns401` | 401 |
| `GetQuiz_ExistingId_Returns200WithQuestionsAndAnswers` | `isCorrect` exposed to admin |
| `GetQuiz_NonExistentId_Returns404` | 404 |
| `CreateQuiz_ValidRequest_Returns201WithId` | 201, ID in body |
| `CreateQuiz_MissingTitle_Returns400` | 400 validation error |
| `CreateQuiz_TitleTooLong_Returns400` | Title max 200 chars |
| `UpdateQuiz_SetPublishedAt_IsPublishedTrue` | Quiz becomes visible to users |
| `UpdateQuiz_ClearPublishedAt_IsPublishedFalse` | Quiz hidden from users |
| `UpdateQuiz_NonExistentId_Returns404` | 404 |
| `DeleteQuiz_ExistingId_Returns204` | 204 No Content |
| `DeleteQuiz_NonExistentId_Returns404` | 404 |

Admin tests call `EvictQuizCacheAsync()` before assertions that depend on fresh data.

---

### Admin/AdminQuestionCrudTests

`/api/admin/quizzes/{id}/questions` — add, update, delete questions within a quiz.

---

### Leaderboard/LeaderboardTests (6 methods)

`GET /api/leaderboard/quizzes` and `GET /api/leaderboard/users`

| Test | Expected |
|------|----------|
| `GetTopQuizzes_Returns200WithRankedList` | 200, ranked list |
| `GetTopQuizzes_Anonymous_Returns200` | No auth required |
| `GetTopQuizzes_CountDefault_Returns10OrFewer` | Default limit 10 |
| `GetTopQuizzes_CountClamped_MaxIs50` | Max limit 50 |
| `GetTopUsers_Returns200WithRankedList` | 200, ranked list |
| `GetTopUsers_Anonymous_Returns200` | No auth required |
