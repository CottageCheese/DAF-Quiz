# Running Playwright Tests Visually

By default Playwright runs headless (no browser window). Three ways to see tests run live:

## 1. Headed Mode

Browser window opens and runs tests at normal speed.

```bash
cd tests/playwright
npx playwright test tests/mvc/public.spec.ts --headed
```

## 2. UI Mode (Recommended)

Interactive test runner with time-travel debugger. Shows DOM snapshots at each step — useful for inspecting axe accessibility violations.

```bash
cd tests/playwright
npx playwright test tests/mvc/public.spec.ts --ui
```

## 3. Debug Mode

Pauses on each step with browser DevTools open. Step through manually.

```bash
cd tests/playwright
npx playwright test tests/mvc/public.spec.ts --debug
```

## Notes

- Config (`playwright.config.ts`) has no `headless: false` — all flags above override at runtime, no config change needed.
- Omit the file path to run all tests in the chosen mode.
- `--ui` is most useful for axe tests — lets you inspect DOM state when accessibility violations fire.
