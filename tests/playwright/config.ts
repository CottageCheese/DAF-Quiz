import * as path from 'path';

export const MVC_BASE_URL = process.env.MVC_BASE_URL ?? 'https://localhost:5001';
export const ANGULAR_BASE_URL = process.env.ANGULAR_BASE_URL ?? 'http://localhost:4200';
export const API_BASE_URL = process.env.API_BASE_URL ?? 'http://localhost:7000';

export const ADMIN_EMAIL = process.env.PLAYWRIGHT_ADMIN_EMAIL ?? 'admin@quiz.local';
export const ADMIN_PASSWORD = process.env.PLAYWRIGHT_ADMIN_PASSWORD ?? '';

export const USER_EMAIL = 'playwright-a11y@test.local';
export const USER_PASSWORD = 'PlaywrightA11y@123!';

const AUTH_DIR = path.join(__dirname, '.auth');
export const MVC_ADMIN_STATE_PATH = path.join(AUTH_DIR, 'mvc-admin.json');
export const MVC_USER_STATE_PATH = path.join(AUTH_DIR, 'mvc-user.json');
export const ANGULAR_USER_STATE_PATH = path.join(AUTH_DIR, 'angular-user.json');
export const TEST_DATA_PATH = path.join(AUTH_DIR, 'test-data.json');
