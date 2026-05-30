import { test } from '@playwright/test';
import * as fs from 'fs';
import { checkPageA11y } from '../../helpers/axe';
import { MVC_ADMIN_STATE_PATH, TEST_DATA_PATH } from '../../config';

test.use({ storageState: MVC_ADMIN_STATE_PATH });

test.describe('MVC admin pages', () => {
  let quizId: number;
  let questionId: number;

  test.beforeAll(async () => {
    const data = JSON.parse(fs.readFileSync(TEST_DATA_PATH, 'utf-8'));
    quizId = data.quizId;
    questionId = data.questionId;
  });

  test('admin quiz list', async ({ page }, testInfo) => {
    await page.goto('/Admin/Index');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('create quiz', async ({ page }, testInfo) => {
    await page.goto('/Admin/Create');
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('quiz details', async ({ page }, testInfo) => {
    await page.goto(`/Admin/Details/${quizId}`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('edit quiz', async ({ page }, testInfo) => {
    await page.goto(`/Admin/Edit/${quizId}`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('add question', async ({ page }, testInfo) => {
    await page.goto(`/Admin/AddQuestion?quizId=${quizId}`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });

  test('edit question', async ({ page }, testInfo) => {
    test.skip(!questionId, 'No question ID available in test data');
    await page.goto(`/Admin/EditQuestion?quizId=${quizId}&questionId=${questionId}`);
    await page.waitForLoadState('networkidle');
    await checkPageA11y(page, testInfo);
  });
});
