import { test } from '@playwright/test';
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
});
