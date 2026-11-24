# Visual regression

Full-page screenshot diff for key MVC pages across 3 viewports (375 / 768 / 1440) and both themes (dark / light).

## Prerequisites

- All four apps running locally (same as the rest of the Playwright suite — see `README.md`).
- Seeded admin + user accounts (auth setup project runs automatically).

## Run

```bash
cd tests/playwright
npm run test:visual            # diff against current baselines
npm run test:visual:update     # regenerate baselines (commit the result)
```

## Baselines

Snapshots are written to:

```
tests/playwright/tests/mvc/visual.spec.ts-snapshots/
```

Chromium rendering is platform-sensitive, so baselines are **platform-stamped** by Playwright (e.g. `-win32-chromium.png`). Commit the baselines produced on the same platform CI runs on; CI should fail if someone commits mismatched platform snapshots.

## CI gate

Add to the CI workflow after the MVC + Angular jobs:

```yaml
- name: Visual regression (MVC)
  working-directory: tests/playwright
  run: npm run test:visual
```

Failing diffs are attached to the HTML report (`playwright-report/index.html`) with pixel-level comparison.

## Scope

- `/` (leaderboard, anon)
- `/Account/Login` (anon)
- `/Dashboard` (authed)
- `/Quiz/Index` (authed)
- `/History` (authed)
- `/Quiz/Results?attemptId=…` (authed)

Admin surface + Angular are out of scope for this first baseline. Add later alongside the Phase 6 Angular port.

## Animations

The spec injects a global style tag freezing all animations/transitions and hides confetti. Combined with `expect.animations: 'disabled'` in `playwright.config.ts`, screenshots are deterministic.
