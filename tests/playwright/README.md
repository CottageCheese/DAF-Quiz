# Playwright + Axe Accessibility Tests

End-to-end accessibility tests for DAF-Quiz using [Playwright](https://playwright.dev/) and [axe-core](https://github.com/dequelabs/axe-core). Every page is scanned against WCAG 2 AA rules via `@axe-core/playwright`.

## Prerequisites

- Node.js 18+
- All three apps running:
  - **API** — `https://localhost:7001` (or `http://localhost:7000`)
  - **MVC** — `https://localhost:5001`
  - **Angular** — `http://localhost:4200`

## Setup

```bash
cd tests/playwright
npm install
npx playwright install chromium
```

Copy `.env.example` to `.env` and fill in the admin password:

```bash
cp .env.example .env
```

`.env` variables:

| Variable | Default | Required |
|---|---|---|
| `PLAYWRIGHT_ADMIN_PASSWORD` | _(none)_ | **Yes** |
| `API_BASE_URL` | `https://localhost:7001` | No |
| `MVC_BASE_URL` | `https://localhost:5001` | No |
| `ANGULAR_BASE_URL` | `http://localhost:4200` | No |

## Running Tests

```bash
# All suites (setup → mvc → angular)
npm test

# MVC only
npm run test:mvc

# Angular only
npm run test:angular

# Open HTML report after a run
npm run report
```

## How It Works

### Auth setup (`tests/auth.setup.ts`)

Runs once before all suites. It:

1. Registers the test user (`playwright-a11y@test.local`) — idempotent, safe to re-run.
2. Logs in as admin and user on MVC, saves cookie storage state to `.auth/`.
3. Logs in as user on Angular, saves `localStorage` state to `.auth/`.
4. Creates a completed quiz attempt via the API and writes `quizId` / `attemptId` to `.auth/test-data.json`.

Storage state files are loaded per project in `playwright.config.ts` — tests never log in manually.

### Test suites

| Suite | Project | Auth |
|---|---|---|
| `tests/mvc/public.spec.ts` | `mvc` | none |
| `tests/mvc/user.spec.ts` | `mvc` | MVC user cookie |
| `tests/mvc/admin.spec.ts` | `mvc` | MVC admin cookie |
| `tests/angular/public.spec.ts` | `angular` | none |
| `tests/angular/user.spec.ts` | `angular` | Angular user localStorage |

Each test navigates to a page, waits for `networkidle`, then calls `checkPageA11y`.

### Axe helper (`helpers/axe.ts`)

```ts
export async function checkPageA11y(page: Page, testInfo: TestInfo): Promise<void>
```

- Runs a full axe scan on the current page.
- Attaches raw violation JSON to the test report as `axe-violations`.
- Fails with `expect.soft` listing `[impact] rule-id: description` per violation.

`expect.soft` means a violation fails the test but does not abort remaining assertions in the same test.

### Projects config (`playwright.config.ts`)

```
setup  →  mvc
       →  angular
```

`setup` must complete before either suite runs. Both suites run in Chrome (`Desktop Chrome` device descriptor). MVC uses `ignoreHTTPSErrors: true` (self-signed cert). Tests run serially (`workers: 1`, `fullyParallel: false`) to avoid shared-state conflicts.

## Fixing Axe Violations

Each violation in the report includes:

- **id** — axe rule (e.g. `color-contrast`, `label`, `button-name`)
- **impact** — `critical`, `serious`, `moderate`, `minor`
- **nodes** — affected HTML with target selector and failure summary
- **helpUrl** — Deque documentation link

Common fixes:

| Rule | Typical cause | Fix |
|---|---|---|
| `color-contrast` | Low contrast text or `opacity < 1` on coloured text | Darken foreground color; remove opacity |
| `label` | `<input>` without associated `<label>` | Add `<label for="id">` or `aria-label` |
| `button-name` | Icon-only button | Add `aria-label` or visually-hidden text |
| `region` | Content outside landmark regions | Wrap in `<main>`, `<nav>`, `<header>` etc. |
| `heading-order` | Skipped heading level | Use sequential `h1 → h2 → h3` |

## CI

Set env vars directly — no `.env` file needed:

```yaml
env:
  PLAYWRIGHT_ADMIN_PASSWORD: ${{ secrets.ADMIN_PASSWORD }}
  API_BASE_URL: http://localhost:7000
  MVC_BASE_URL: http://localhost:5001
  ANGULAR_BASE_URL: http://localhost:4200
```

`forbidOnly: true` and `retries: 1` are enabled automatically when `CI=true`.
