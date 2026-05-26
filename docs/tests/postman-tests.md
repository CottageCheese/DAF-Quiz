# Postman / Newman Tests

End-to-end tests against a live running API. Uses [Newman](https://github.com/postmanlabs/newman) (Postman's CLI runner).

Files live in `tests/postman/`.

## Prerequisites

- Node.js + npm installed
- API running locally (default: `http://localhost:5169`)
- Admin and user accounts exist (seeded via `SeedData.InitialiseAsync` — `admin@quiz.local`)

## Run

### PowerShell script (recommended)

```powershell
./scripts/run-postman-tests.ps1 -AdminPassword "Admin123!" -UserPassword "User123!"
```

Full parameter list:

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-AdminPassword` | *(required)* | Admin account password |
| `-UserPassword` | *(required)* | User account password |
| `-AdminEmail` | `admin@quiz.local` | Admin email |
| `-UserEmail` | `user@quiz.local` | User email |
| `-BaseUrl` | `http://localhost:5169` | API base URL |
| `-CollectionFile` | `tests/postman/DAF-Quiz.postman_collection.json` | Collection path |
| `-EnvironmentFile` | `tests/postman/DAF-Quiz.postman_environment.json` | Environment path |
| `-JunitOutput` | `tests/postman/results.xml` | JUnit XML report output path |

The script installs Newman globally (`npm install --global newman`) then runs the collection. Throws on test failure.

### Direct Newman command

```bash
npx newman run tests/postman/DAF-Quiz.postman_collection.json \
  --environment tests/postman/DAF-Quiz.postman_environment.json \
  --env-var "adminEmail=admin@quiz.local" \
  --env-var "adminPassword=Admin123!" \
  --env-var "userEmail=user@quiz.local" \
  --env-var "userPassword=User123!" \
  --env-var "baseUrl=http://localhost:5169" \
  --reporters cli,junit \
  --reporter-junit-export tests/postman/results.xml
```

## Collection Structure

The collection runs a full E2E flow in sequence:

1. **Register** — creates a unique test user (email/displayName timestamped)
2. **Login (User)** — stores `accessToken` + `refreshToken`
3. **Login (Admin)** — stores `adminAccessToken`
4. **Create Quiz** — admin creates a quiz, stores `quizId`
5. **Add Question** — admin adds a question with answers
6. **Publish Quiz** — admin sets `publishedAt`
7. **Get Published Quizzes** — user sees the quiz
8. **Start Attempt** — user starts an attempt, stores `attemptId`
9. **Submit Attempt** — user submits answers
10. **Get Result** — user retrieves score and grade
11. **Top Quizzes** — anonymous leaderboard check
12. **Top Users** — anonymous leaderboard check

Each request has inline test scripts validating status codes and response shapes.

## Token Auto-Refresh

A pre-request script on the collection checks if `accessToken` is expiring within 60 seconds. If so, it calls `POST /api/auth/refresh` automatically before the request fires. Auth endpoints are skipped to avoid recursion.

This means long-running test runs handle token expiry transparently.

## Environment File

`tests/postman/DAF-Quiz.postman_environment.json` is committed with empty secret values. Never commit real passwords.

Credentials are injected at runtime via `--env-var` flags (script or CLI).

## CI (GitHub Actions)

Pass credentials from repository secrets:

```yaml
- name: Run Postman tests
  run: ./scripts/run-postman-tests.ps1 -AdminPassword "${{ secrets.ADMIN_PASSWORD }}" -UserPassword "${{ secrets.USER_PASSWORD }}"
  shell: pwsh
```

JUnit output at `tests/postman/results.xml` can be published with any JUnit reporter action.

## Output

Newman prints a summary table to console. JUnit XML is written to the path specified by `-JunitOutput`.

Example console output:

```
┌─────────────────────────┬───────────────────┬───────────────────┐
│                         │          executed │            failed │
├─────────────────────────┼───────────────────┼───────────────────┤
│              iterations │                 1 │                 0 │
│                requests │                12 │                 0 │
│            test-scripts │                12 │                 0 │
│      prerequest-scripts │                12 │                 0 │
│              assertions │                28 │                 0 │
└─────────────────────────┴───────────────────┴───────────────────┘
```
