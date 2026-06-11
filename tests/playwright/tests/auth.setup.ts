import { test as setup, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';
import {
  MVC_BASE_URL, MVC_ADMIN_BASE_URL, ANGULAR_BASE_URL, API_BASE_URL,
  ADMIN_EMAIL, ADMIN_PASSWORD, USER_EMAIL, USER_PASSWORD,
  MVC_ADMIN_STATE_PATH, MVC_USER_STATE_PATH,
  ANGULAR_USER_STATE_PATH, TEST_DATA_PATH,
} from '../config';

setup('prepare accessibility test state', async ({ browser, request }) => {
  fs.mkdirSync(path.dirname(MVC_ADMIN_STATE_PATH), { recursive: true });

  // --- 1. Register test user (idempotent — 400/409 means already exists) ---
  const registerResp = await request.post(`${API_BASE_URL}/api/auth/register`, {
    data: { email: USER_EMAIL, password: USER_PASSWORD, displayName: 'A11y Test User' },
  });
  expect([200, 201, 400, 409]).toContain(registerResp.status());

  // --- 2. MVC admin login ---
  {
    const ctx = await browser.newContext({ ignoreHTTPSErrors: true });
    const page = await ctx.newPage();
    await page.goto(`${MVC_ADMIN_BASE_URL}/Account/Login`);
    await page.locator('#Email').fill(ADMIN_EMAIL);
    await page.locator('#Password').fill(ADMIN_PASSWORD);
    await page.locator('[type="submit"]').click();
    await page.waitForURL(u => !u.toString().includes('/Account/Login'));
    await ctx.storageState({ path: MVC_ADMIN_STATE_PATH });
    await ctx.close();
  }

  // --- 3. MVC user login ---
  {
    const ctx = await browser.newContext({ ignoreHTTPSErrors: true });
    const page = await ctx.newPage();
    await page.goto(`${MVC_BASE_URL}/Account/Login`);
    await page.locator('#Email').fill(USER_EMAIL);
    await page.locator('#Password').fill(USER_PASSWORD);
    await page.locator('[type="submit"]').click();
    await page.waitForURL(u => !u.toString().includes('/Account/Login'));
    await ctx.storageState({ path: MVC_USER_STATE_PATH });
    await ctx.close();
  }

  // --- 4. Angular user login (tokens saved via localStorage in storageState) ---
  {
    const ctx = await browser.newContext();
    const page = await ctx.newPage();
    await page.goto(`${ANGULAR_BASE_URL}/login`);
    await page.locator('input[type="email"]').fill(USER_EMAIL);
    await page.locator('input[type="password"]').fill(USER_PASSWORD);
    await page.locator('button[type="submit"]').click();
    await page.waitForURL(u => !u.toString().includes('/login'), { timeout: 10_000 });
    await ctx.storageState({ path: ANGULAR_USER_STATE_PATH });
    await ctx.close();
  }

  // --- 5. Fetch test data via API ---
  const adminLoginResp = await request.post(`${API_BASE_URL}/api/auth/login`, {
    data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
  });
  expect(adminLoginResp.ok()).toBeTruthy();
  const { accessToken: adminToken } = await adminLoginResp.json();

  // Get admin quiz list to find a quiz ID
  const quizzesResp = await request.get(`${API_BASE_URL}/api/admin/quizzes`, {
    headers: { Authorization: `Bearer ${adminToken}` },
  });
  expect(quizzesResp.ok()).toBeTruthy();
  const quizzesData = await quizzesResp.json();
  const items: { id: number; isPublished: boolean }[] = quizzesData.items ?? quizzesData;
  const quizId: number = items.find(q => q.isPublished)?.id ?? items[0]?.id;
  expect(quizId).toBeTruthy();

  // Get question ID from admin quiz detail
  const quizDetailResp = await request.get(`${API_BASE_URL}/api/admin/quizzes/${quizId}`, {
    headers: { Authorization: `Bearer ${adminToken}` },
  });
  const quizDetail = await quizDetailResp.json();
  const questionId: number = quizDetail.questions?.[0]?.id;

  // Create a completed attempt as the test user
  const userLoginResp = await request.post(`${API_BASE_URL}/api/auth/login`, {
    data: { email: USER_EMAIL, password: USER_PASSWORD },
  });
  const { accessToken: userToken } = await userLoginResp.json();

  const startResp = await request.post(`${API_BASE_URL}/api/quizzes/${quizId}/start`, {
    headers: { Authorization: `Bearer ${userToken}` },
  });
  expect(startResp.ok()).toBeTruthy();
  const quizData = await startResp.json();
  const attemptId: number = quizData.attemptId;

  const selections = quizData.questions.map((q: { questionId: number; answers: { answerId: number }[] }) => ({
    questionId: q.questionId,
    selectedAnswerId: q.answers[0].answerId,
  }));
  const submitResp = await request.post(`${API_BASE_URL}/api/quizzes/attempts/${attemptId}/submit`, {
    headers: { Authorization: `Bearer ${userToken}` },
    data: selections,
  });
  expect(submitResp.ok()).toBeTruthy();

  fs.writeFileSync(TEST_DATA_PATH, JSON.stringify({ quizId, questionId, attemptId }, null, 2));
});
