# Unit Tests

Two projects cover unit-level testing. No HTTP, no running servers — just services wired to an in-memory SQLite database.

## Projects

| Project | Tests Service(s) |
|---------|-----------------|
| `QuizProject.Domain.Tests` | `QuizService`, `AdminQuizService`, `LeaderboardService` |
| `QuizProject.Api.Tests` | `TokenService` (JWT) |

## Run

```bash
# Both projects
dotnet test QuizProject.Domain.Tests
dotnet test QuizProject.Api.Tests

# Single test by name
dotnet test QuizProject.Domain.Tests --filter "FullyQualifiedName~SubmitAttemptAsync_AllCorrect"
```

## Infrastructure

### DomainTestBase (`QuizProject.Domain.Tests/Infrastructure/`)

Abstract base class all domain tests extend.

```csharp
public abstract class DomainTestBase : IDisposable
{
    protected ApplicationDbContext Db { get; }
    protected IDistributedCache Cache { get; }  // MemoryDistributedCache
}
```

Each test class gets a fresh in-memory SQLite connection. Disposed after every test.

### DomainTestSeeder

Seeds a minimal fixture for domain tests:

| Seed | Value |
|------|-------|
| User | `testuser@test.local` |
| Published quiz | 2 questions, each with 1 correct + 1 wrong answer |
| Draft quiz | No `PublishedAt` |

Returns a `SeedContext` record exposing `User`, `PublishedQuiz`, `DraftQuiz`, `QuestionIds[]`, `CorrectAnswerIds[]`, `WrongAnswerIds[]`.

### ApiTestBase (`QuizProject.Api.Tests/Infrastructure/`)

Extends domain test infrastructure. Adds:

```csharp
protected UserManager<ApplicationUser> UserManager { get; }
protected TokenService TokenService { get; }
```

Static JWT settings used in tests:

```
Issuer:   test-issuer
Audience: test-audience
Secret:   super-secret-key-for-testing-only-32chars!!
Access:   15 min expiry
Refresh:  7 day expiry
```

Helper: `CreateUserAsync(email, role)` — creates a user and assigns the given role.

---

## Test Classes

### QuizServiceTests (8 methods)

Tests `QuizService` — what regular users interact with.

| Test | Verifies |
|------|----------|
| `GetActiveQuizzesAsync_ReturnsPublished_ExcludesDraft` | Only quizzes with `PublishedAt <= now` returned |
| `GetActiveQuizzesAsync_ExcludesFuturePublishedAt` | Future `PublishedAt` excluded |
| `StartAttemptAsync_Published_ReturnsViewModel` | Attempt initialised for published quiz |
| `StartAttemptAsync_Draft_ReturnsNull` | Draft quiz returns null |
| `SubmitAttemptAsync_AllCorrect_ScoreEqualsTotal` | Perfect score calculated correctly |
| `SubmitAttemptAsync_AllWrong_ScoreIsZero` | Zero score on all wrong answers |
| `GetResultAsync_CompletedAttempt_ReturnsResult` | Result retrieved after completion |
| `GetResultAsync_WrongUser_ReturnsNull` | Users cannot view other users' results |

Plus 5 inline theory tests on `QuizResultViewModel.Grade`:

| Score | Label |
|-------|-------|
| 90–100% | Excellent |
| 70–89% | Good |
| 50–69% | Pass |
| < 50% | Needs Improvement |

---

### AdminQuizServiceTests (11 methods)

Tests `AdminQuizService` — admin CRUD over quizzes and questions.

| Test | Verifies |
|------|----------|
| `GetAllQuizzesAsync_ReturnsBothPublishedAndDraft` | Admin sees all quizzes regardless of `PublishedAt` |
| `GetAllQuizzesAsync_OrderedByCreatedAtDescending` | Newest first |
| `CreateQuizAsync_PersistsWithCorrectMetadata` | Creates with correct fields |
| `UpdateQuizAsync_UpdatesFields` | Title, description, `PublishedAt` all update |
| `UpdateQuizAsync_NonExistent_ReturnsNull` | Returns null for missing quiz |
| `DeleteQuizAsync_RemovesQuiz` | Quiz removed from DB |
| `DeleteQuizAsync_NonExistent_ReturnsFalse` | Returns false for missing quiz |
| `AddQuestionAsync_PersistsQuestionAndAnswers` | Creates question with answers |
| `UpdateQuestionAsync_ReplacesAllAnswers` | Update replaces entire answer set |
| `DeleteQuestionAsync_NonExistent_ReturnsFalse` | Returns false for missing question |

---

### LeaderboardServiceTests (4 methods)

Tests `LeaderboardService` — ranking logic.

| Test | Verifies |
|------|----------|
| `GetTopQuizzesAsync_RankedByCompletedAttemptCount` | Quizzes ranked by completion count |
| `GetTopQuizzesAsync_ExcludesIncompleteAttempts` | Only completed attempts count |
| `GetTopUsersAsync_RankedByBestScorePercent` | Users ranked by best score percentage |
| `GetTopUsersAsync_BestScorePercentRoundedToOneDecimal` | Score rounded to 1 decimal place |

---

### TokenServiceTests (16 methods)

Tests `TokenService` — JWT access tokens and refresh token lifecycle.

**GenerateAccessTokenAsync**

| Test | Verifies |
|------|----------|
| `GenerateAccessTokenAsync_ContainsExpectedClaims` | JWT contains `sub`, `email` claims |
| `GenerateAccessTokenAsync_TokenIsValid` | Token is well-formed |

**CreateRefreshTokenAsync**

| Test | Verifies |
|------|----------|
| `CreateRefreshTokenAsync_PersistsHashedToken_RawNotStored` | Raw token never stored, only hash |
| `CreateRefreshTokenAsync_ExpirySetCorrectly` | 7-day expiry window |

**RotateRefreshTokenAsync**

| Test | Verifies |
|------|----------|
| `RotateRefreshTokenAsync_ValidToken_ReturnsNewTokenPair` | Fresh token pair on valid rotation |
| `RotateRefreshTokenAsync_ValidToken_MarksOldTokenAsUsed` | Old token flagged as used |
| `RotateRefreshTokenAsync_UsedToken_RevokesAllAndReturnsNull` | Reuse detection — all tokens revoked |
| `RotateRefreshTokenAsync_ExpiredToken_ReturnsNull` | Expired tokens rejected |
| `RotateRefreshTokenAsync_UnknownToken_ReturnsNull` | Unknown token rejected |

**RevokeRefreshTokenAsync**

| Test | Verifies |
|------|----------|
| `RevokeRefreshTokenAsync_ActiveToken_SetsRevokedAt` | Sets `RevokedAt` timestamp |
| `RevokeRefreshTokenAsync_AlreadyRevoked_ReturnsFalse` | Can't revoke twice |
| `RevokeRefreshTokenAsync_UnknownToken_ReturnsFalse` | Returns false for missing token |

**RevokeAllRefreshTokensForUserAsync**

| Test | Verifies |
|------|----------|
| `RevokeAllRefreshTokensForUserAsync_RevokesOnlyActiveTokens` | Only active tokens revoked; already-revoked left alone |
