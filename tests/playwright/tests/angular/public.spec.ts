import { test } from '@playwright/test';
import { checkPageA11y } from '../../helpers/axe';

test.describe('Angular public pages', () => {
  test('login', async ({ page }, testInfo) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('register', async ({ page }, testInfo) => {
    await page.goto('/register');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('leaderboard', async ({ page }, testInfo) => {
    await page.goto('/leaderboard');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });
});
