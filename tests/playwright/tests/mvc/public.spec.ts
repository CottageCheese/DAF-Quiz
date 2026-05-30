import { test } from '@playwright/test';
import { checkPageA11y } from '../../helpers/axe';

test.describe('MVC public pages', () => {
  test('home / leaderboard', async ({ page }, testInfo) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('login', async ({ page }, testInfo) => {
    await page.goto('/Account/Login');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('register', async ({ page }, testInfo) => {
    await page.goto('/Account/Register');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('access denied', async ({ page }, testInfo) => {
    await page.goto('/Account/AccessDenied');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });
});
