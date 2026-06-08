// Captures redesign screenshots (desktop + mobile) against the running Vite dev
// server with the API stubbed at the network layer (no backend needed).
//   node scripts/screenshot.mjs            -> after-desktop.png, after-mobile.png
// Pass a label to name the files, e.g. `node scripts/screenshot.mjs before`.
import { chromium } from '@playwright/test';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';

const label = process.argv[2] ?? 'after';
const BASE = 'http://localhost:5173';
const outDir = fileURLToPath(new URL('../docs/', import.meta.url));

async function stub(page) {
  await page.route('**/api/lookups/governorates', (r) =>
    r.fulfill({ json: [
      { id: 1, name: 'Cairo' },
      { id: 2, name: 'Giza' },
      { id: 3, name: 'Alexandria' },
    ] }),
  );
  await page.route('**/api/lookups/cities*', (r) =>
    r.fulfill({ json: [
      { id: 101, governorateId: 1, name: 'Nasr City' },
      { id: 102, governorateId: 1, name: 'Maadi' },
    ] }),
  );
  await page.route('**/api/registrations', (r) =>
    r.fulfill({ status: 201, json: { id: '11111111-2222-3333-4444-555555555555' } }),
  );
}

async function shoot(browser, { width, height, name }) {
  const ctx = await browser.newContext({ viewport: { width, height }, deviceScaleFactor: 2 });
  const page = await ctx.newPage();
  await stub(page);
  await page.goto(BASE, { waitUntil: 'networkidle' });
  await page.getByRole('option', { name: 'Cairo' }).waitFor({ state: 'attached' });
  await page.waitForTimeout(900); // let the entrance animation settle
  await page.screenshot({ path: `${outDir}${name}`, fullPage: true });
  await ctx.close();
  console.log('wrote', name);
}

await mkdir(outDir, { recursive: true });
const browser = await chromium.launch();
await shoot(browser, { width: 1280, height: 1000, name: `${label}-desktop.png` });
await shoot(browser, { width: 375, height: 812, name: `${label}-mobile.png` });
await browser.close();
