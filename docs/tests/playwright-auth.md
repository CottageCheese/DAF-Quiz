# Playwright Authentication

Playwright tests use a **setup-then-reuse** pattern. A dedicated setup project performs real browser logins once, saves the resulting auth state to JSON files, and all test projects load those files — no login code needed in individual tests.

## Prerequisites

API and Web must be running before Playwright executes:

```bash
# Terminal 1
cd QuizProject.Api && dotnet run

# Terminal 2
cd QuizProject.Web && dotnet run
```

## Two-Phase Execution

### Phase 1: `setup` project (`tests/auth.setup.ts`)

Runs once before all other projects (enforced via `dependencies: ['setup']` in `playwright.config.ts`).

1. **Register test user** — `POST /api/auth/register` with hardcoded credentials (ignores 409 if already exists)
2. **MVC Admin login** — navigates to `/Account/Login`, fills `#Email`/`#Password`, submits, saves cookies
3. **MVC User login** — same flow with user credentials
4. **Angular User login** — navigates to `/login`, fills `input[type="email"]`/`input[type="password"]`, saves localStorage tokens
5. **Seed test data** — hits API directly as admin to create a quiz attempt; saves IDs to `test-data.json`

### Phase 2: Test projects (`mvc`, `angular`)

Each test file declares which saved state to inject:

```ts
test.use({ storageState: MVC_USER_STATE_PATH });   // authenticated as user
test.use({ storageState: MVC_ADMIN_STATE_PATH });  // authenticated as admin
// no storageState call = unauthenticated (public pages)
```

Playwright loads the JSON into the browser context before any test runs. The page opens already authenticated.

## Auth State Files

All saved to `tests/playwright/.auth/` (git-ignored):

| File | Contents | Used by |
|------|----------|---------|
| `mvc-admin.json` | ASP.NET Core cookies (auth, session, CSRF) | `mvc/admin.spec.ts` |
| `mvc-user.json` | ASP.NET Core cookies (user role) | `mvc/user.spec.ts` |
| `angular-user.json` | JWT access + refresh tokens in `localStorage` | `angular/user.spec.ts` |
| `test-data.json` | Pre-created `quizId`, `questionId`, `attemptId` | All spec files via `beforeAll` |

## Why It Works

| Framework | Auth mechanism | What gets saved |
|-----------|---------------|-----------------|
| MVC | Cookie-based (httpOnly) | Browser cookies = active session |
| Angular | JWT in `localStorage` | Tokens = authenticated API calls |

`storageState` captures both, so Playwright can restore either.

## Credentials

Configured in `tests/playwright/config.ts`:

- **Admin**: from env vars `PLAYWRIGHT_ADMIN_EMAIL` / `PLAYWRIGHT_ADMIN_PASSWORD`
- **Test user**: hardcoded `playwright-a11y@test.local` / `PlaywrightA11y@123!`

## Key Files

| File | Purpose |
|------|---------|
| `tests/playwright/config.ts` | URLs, credentials, state file paths |
| `tests/playwright/playwright.config.ts` | Project definitions, setup dependency |
| `tests/playwright/tests/auth.setup.ts` | One-time login + test data seeding |
| `tests/playwright/.auth/*.json` | Stored auth states (generated, git-ignored) |
