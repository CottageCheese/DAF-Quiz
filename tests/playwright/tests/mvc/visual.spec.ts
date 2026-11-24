import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import { MVC_USER_STATE_PATH, TEST_DATA_PATH } from '../../config';

/**
 * Visual regression (P5.1 / #32).
 * Captures full-page screenshots of key pages across:
 *   - viewports: 375 (mobile), 768 (tablet), 1440 (desktop)
 *   - themes:    dark, light
 *
 * Baselines live in tests/__snapshots__/visual.spec.ts-snapshots/.
 * Update with: npx playwright test visual --update-snapshots
 */

type Viewport = { name: string; width: number; height: number };
type Theme = 'dark' | 'light';

const VIEWPORTS: Viewport[] = [
  { name: '375', width: 375, height: 800 },
  { name: '768', width: 768, height: 1024 },
  { name: '1440', width: 1440, height: 900 },
];

const THEMES: Theme[] = ['dark', 'light'];

async function freezeAnimations(page: import('@playwright/test').Page) {
  await page.addStyleTag({
    content: `
      *, *::before, *::after {
        animation-duration: 0s !important;
        animation-delay: 0s !important;
        transition-duration: 0s !important;
        transition-delay: 0s !important;
        scroll-behavior: auto !important;
      }
      .confetti, #result-confetti { display: none !important; }
      .caret, .cursor { visibility: hidden !important; }
    `,
  });
}

async function setTheme(page: import('@playwright/test').Page, theme: Theme) {
  await page.addInitScript((t) => {
    try { localStorage.setItem('theme', t); } catch { /* ignore */ }
  }, theme);
}

test.describe('Visual regression — public anonymous', () => {
  for (const vp of VIEWPORTS) {
    for (const theme of THEMES) {
      test(`leaderboard @ ${vp.name}w ${theme}`, async ({ page }) => {
        await setTheme(page, theme);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto('/');
        await page.waitForLoadState('networkidle');
        await freezeAnimations(page);
        await expect(page).toHaveScreenshot(`leaderboard-${vp.name}-${theme}.png`, {
          fullPage: true,
          maxDiffPixelRatio: 0.01,
        });
      });

      test(`login @ ${vp.name}w ${theme}`, async ({ page }) => {
        await setTheme(page, theme);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto('/Account/Login');
        await page.waitForLoadState('networkidle');
        await freezeAnimations(page);
        await expect(page).toHaveScreenshot(`login-${vp.name}-${theme}.png`, {
          fullPage: true,
          maxDiffPixelRatio: 0.01,
        });
      });
    }
  }
});

test.describe('Visual regression — user authed', () => {
  test.use({ storageState: MVC_USER_STATE_PATH });

  let attemptId: number;
  test.beforeAll(async () => {
    const data = JSON.parse(fs.readFileSync(TEST_DATA_PATH, 'utf-8'));
    attemptId = data.attemptId;
  });

  for (const vp of VIEWPORTS) {
    for (const theme of THEMES) {
      test(`dashboard @ ${vp.name}w ${theme}`, async ({ page }) => {
        await setTheme(page, theme);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto('/Dashboard');
        await page.waitForLoadState('networkidle');
        await freezeAnimations(page);
        await expect(page).toHaveScreenshot(`dashboard-${vp.name}-${theme}.png`, {
          fullPage: true,
          maxDiffPixelRatio: 0.01,
        });
      });

      test(`quiz list @ ${vp.name}w ${theme}`, async ({ page }) => {
        await setTheme(page, theme);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto('/Quiz/Index');
        await page.waitForLoadState('networkidle');
        await freezeAnimations(page);
        await expect(page).toHaveScreenshot(`quiz-list-${vp.name}-${theme}.png`, {
          fullPage: true,
          maxDiffPixelRatio: 0.01,
        });
      });

      test(`history @ ${vp.name}w ${theme}`, async ({ page }) => {
        await setTheme(page, theme);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto('/History');
        await page.waitForLoadState('networkidle');
        await freezeAnimations(page);
        await expect(page).toHaveScreenshot(`history-${vp.name}-${theme}.png`, {
          fullPage: true,
          maxDiffPixelRatio: 0.01,
        });
      });

      test(`quiz results @ ${vp.name}w ${theme}`, async ({ page }) => {
        await setTheme(page, theme);
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto(`/Quiz/Results?attemptId=${attemptId}`);
        await page.waitForLoadState('networkidle');
        await freezeAnimations(page);
        // Wait for score count-up + ring fill to settle (frozen animations = instant)
        await page.waitForTimeout(100);
        await expect(page).toHaveScreenshot(`quiz-results-${vp.name}-${theme}.png`, {
          fullPage: true,
          maxDiffPixelRatio: 0.015, // ring is anti-aliased, allow slight slack
        });
      });
    }
  }
});
