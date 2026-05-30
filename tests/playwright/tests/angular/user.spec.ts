import { test } from '@playwright/test';
import * as fs from 'fs';
import { checkPageA11y } from '../../helpers/axe';
import { ANGULAR_USER_STATE_PATH, TEST_DATA_PATH } from '../../config';

test.use({ storageState: ANGULAR_USER_STATE_PATH });

test.describe('Angular user pages', () => {
  let quizId: number;
  let attemptId: number;

  test.beforeAll(async () => {
    const data = JSON.parse(fs.readFileSync(TEST_DATA_PATH, 'utf-8'));
    quizId = data.quizId;
    attemptId = data.attemptId;
  });

  test('quiz list', async ({ page }, testInfo) => {
    await page.goto('/quizzes');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('take quiz', async ({ page }, testInfo) => {
    // Navigating directly starts an attempt; axe scans the quiz-taking UI
    await page.goto(`/quizzes/${quizId}/take`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('quiz result', async ({ page }, testInfo) => {
    await page.goto(`/quizzes/attempts/${attemptId}/result`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('history', async ({ page }, testInfo) => {
    await page.goto('/history');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });
});
