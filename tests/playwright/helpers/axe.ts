import { Page, TestInfo, expect } from '@playwright/test';
import { AxeBuilder } from '@axe-core/playwright';

export async function checkPageA11y(page: Page, testInfo: TestInfo): Promise<void> {
  const results = await new AxeBuilder({ page }).analyze();

  await testInfo.attach('axe-violations', {
    body: JSON.stringify(results.violations, null, 2),
    contentType: 'application/json',
  });

  if (results.violations.length > 0) {
    const summary = results.violations
      .map(v => `[${v.impact}] ${v.id}: ${v.description}\n  Nodes: ${v.nodes.length}`)
      .join('\n');
    expect.soft(results.violations, `Axe violations on ${page.url()}:\n${summary}`).toHaveLength(0);
  }
}
