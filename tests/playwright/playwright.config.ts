import { defineConfig, devices } from '@playwright/test';
import { config } from 'dotenv';
import { MVC_BASE_URL, MVC_ADMIN_BASE_URL, ANGULAR_BASE_URL } from './config';

config(); // loads .env if present (ignored in CI where env vars are set directly)

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  reporter: [['html', { open: 'never' }], ['list']],

  // Visual regression baselines live next to the specs.
  // Chromium rendering can differ slightly by platform — tolerance below
  // combined with per-test maxDiffPixelRatio keeps CI stable.
  expect: {
    toHaveScreenshot: {
      maxDiffPixelRatio: 0.01,
      animations: 'disabled',
      caret: 'hide',
    },
  },

  projects: [
    {
      name: 'setup',
      testMatch: /auth\.setup\.ts/,
      use: { ignoreHTTPSErrors: true },
    },
    {
      name: 'mvc',
      testMatch: /mvc\/(?!admin).+\.spec\.ts/,
      dependencies: ['setup'],
      use: {
        ...devices['Desktop Chrome'],
        baseURL: MVC_BASE_URL,
        ignoreHTTPSErrors: true,
      },
    },
    {
      name: 'mvc-admin',
      testMatch: /mvc\/admin\.spec\.ts/,
      dependencies: ['setup'],
      use: {
        ...devices['Desktop Chrome'],
        baseURL: MVC_ADMIN_BASE_URL,
        ignoreHTTPSErrors: true,
      },
    },
    {
      name: 'angular',
      testMatch: /angular\/.+\.spec\.ts/,
      dependencies: ['setup'],
      use: {
        ...devices['Desktop Chrome'],
        baseURL: ANGULAR_BASE_URL,
      },
    },
  ],
});
