# DAF-Quiz Postman Tests

## Local Setup

1. Import `DAF-Quiz.postman_collection.json` and `DAF-Quiz.postman_environment.json` into Postman.
2. Select the **DAF-Quiz Local** environment.
3. Fill in the secret values that are intentionally blank in the committed file:
   - `adminPassword` — the Admin account password (configured in `appsettings.json` → `AdminSettings:Password`)
   - `adminEmail` — default is `admin@quiz.local` (already populated)
   - `userEmail` / `userPassword` — a regular user account (create one via the Register request first)
4. Run **Admin Login** first to populate `adminAccessToken`.
5. Run **Login (User)** to populate `accessToken`.

## E2E Flow (run in order)

1. Admin Login
2. Create Quiz → sets `quizId`
3. Add Question (run 3×, adjusting body) → sets `questionId`, `correctAnswerId`
4. Publish Quiz (PUT with `publishedAt`)
5. Login (User) → sets `accessToken`
6. Get Published Quizzes — verify your quiz appears
7. Start Attempt → sets `attemptId`, `firstQuestionId`, `firstAnswerId`
8. Submit Attempt
9. Get Result
10. Top Quizzes / Top Users

## Running via Newman (CLI)

```bash
npm install -g newman newman-reporter-junit

newman run DAF-Quiz.postman_collection.json \
  --environment DAF-Quiz.postman_environment.json \
  --env-var "adminPassword=<your-password>" \
  --env-var "userPassword=<user-password>" \
  --reporters cli,junit \
  --reporter-junit-export results.xml
```

## Running via PowerShell script (recommended)

From the repository root:

```powershell
./scripts/run-postman-tests.ps1 `
  -AdminPassword "<admin-password>" `
  -UserPassword "<user-password>" `
  -AdminEmail "admin@quiz.local" `
  -UserEmail "user@quiz.local" `
  -BaseUrl "http://localhost:5169"
```

This installs Newman locally in the repo (`devDependencies`) and writes a JUnit report to `tests/postman/results.xml`.

## GitHub Actions

Workflow file: `.github/workflows/postman-tests.yml`

Required repository secrets:

- `POSTMAN_ADMIN_PASSWORD`
- `POSTMAN_USER_PASSWORD`

Optional repository variables:

- `POSTMAN_ADMIN_EMAIL` (defaults to `admin@quiz.local`)
- `POSTMAN_USER_EMAIL` (defaults to `user@quiz.local`)

## Version Control Rules

- `DAF-Quiz.postman_collection.json` — **committed** (no secrets, all test logic)
- `DAF-Quiz.postman_environment.json` — **committed** (all secret values are empty strings)
- Never commit a populated environment file with real passwords or tokens
- CI injects secrets via `--env-var` flags from pipeline secret variables
