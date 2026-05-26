# Test Overview

Three test suites at different layers of the stack.

| Suite | Project | Scope | Speed |
|-------|---------|-------|-------|
| Unit | `QuizProject.Domain.Tests`, `QuizProject.Api.Tests` | Services in isolation, SQLite in-memory | Fast |
| Integration | `QuizProject.Tests.Integration` | Full HTTP stack via WebApplicationFactory | Medium |
| Postman/Newman | `tests/postman/` | E2E against a running API | Slow |

## Quick Run

```bash
# All unit + integration tests
dotnet test QuizProject.sln

# Domain unit tests only
dotnet test QuizProject.Domain.Tests

# API unit tests only
dotnet test QuizProject.Api.Tests

# Integration tests only
dotnet test QuizProject.Tests.Integration

# Postman/Newman E2E (API must be running)
./scripts/run-postman-tests.ps1 -AdminPassword "Admin123!" -UserPassword "User123!"
```

## What Each Suite Covers

**Unit tests** verify business logic in isolation — scoring, grading, leaderboard ranking, token generation/rotation. No HTTP involved.

**Integration tests** verify the HTTP surface — status codes, response shapes, auth enforcement, admin authorization, quiz visibility rules.

**Postman tests** verify the full E2E flow — register, login, create quiz, publish, take quiz, submit, check leaderboard — against a live running server.

## Further Reading

- [Unit Tests](unit-tests.md) — Domain + API test structure, base classes, seeder
- [Integration Tests](integration-tests.md) — WebApplicationFactory setup, test collections, seeded data
- [Postman Tests](postman-tests.md) — Newman CLI, environment variables, CI setup
