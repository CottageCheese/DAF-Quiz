import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import { checkPageA11y } from '../../helpers/axe';
import { MVC_USER_STATE_PATH, TEST_DATA_PATH } from '../../config';

test.use({ storageState: MVC_USER_STATE_PATH });

test.describe('MVC user pages', () => {
  let attemptId: number;

  test.beforeAll(async () => {
    const data = JSON.parse(fs.readFileSync(TEST_DATA_PATH, 'utf-8'));
    attemptId = data.attemptId;
  });

  test('quiz list', async ({ page }, testInfo) => {
    await page.goto('/Quiz/Index');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('quiz results', async ({ page }, testInfo) => {
    await page.goto(`/Quiz/Results?attemptId=${attemptId}`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('dashboard', async ({ page }, testInfo) => {
    await page.goto('/Dashboard');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('history (all)', async ({ page }, testInfo) => {
    await page.goto('/History');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('history (passed filter)', async ({ page }, testInfo) => {
    await page.goto('/History?filter=passed');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('keyboard: topbar nav tabs through visible focus rings', async ({ page }) => {
    await page.goto('/Dashboard');
    await page.waitForLoadState('networkidle');

    // Skip link first
    await page.keyboard.press('Tab');
    const skip = page.locator('a.skip-link');
    await expect(skip).toBeFocused();

    // Tab across topbar links — focus must stay visible and follow DOM order
    for (let i = 0; i < 6; i++) {
      await page.keyboard.press('Tab');
      const focused = await page.evaluate(() => {
        const el = document.activeElement as HTMLElement | null;
        if (!el) return null;
        const rect = el.getBoundingClientRect();
        return { tag: el.tagName, visible: rect.width > 0 && rect.height > 0 };
      });
      expect(focused?.visible, `step ${i}: focused element must be visible`).toBe(true);
    }
  });
});
