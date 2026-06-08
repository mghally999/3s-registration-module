import { expect, Page, test } from '@playwright/test';

// stub the three api calls the form makes so the e2e flow does not need a live
// backend. the happy-path stub returns a created id; we never touch a database.
async function stubApi(page: Page) {
  await page.route('**/api/lookups/governorates', (route) =>
    route.fulfill({
      json: [
        { id: 1, name: 'Cairo' },
        { id: 2, name: 'Giza' },
      ],
    }),
  );

  await page.route('**/api/lookups/cities*', (route) =>
    route.fulfill({
      json: [
        { id: 101, governorateId: 1, name: 'Nasr City' },
        { id: 102, governorateId: 1, name: 'Maadi' },
      ],
    }),
  );

  await page.route('**/api/registrations', (route) =>
    route.fulfill({
      status: 201,
      json: { id: '11111111-2222-3333-4444-555555555555' },
    }),
  );
}

const adultBirthDate = `${new Date().getFullYear() - 25}-04-12`;

test.describe('registration form', () => {
  test.beforeEach(async ({ page }) => {
    await stubApi(page);
    await page.goto('/');
  });

  test('happy path: a valid registration is submitted and confirmed', async ({ page }) => {
    await page.getByLabel(/first name/i).fill('Mohammed');
    await page.getByLabel(/last name/i).fill('Ghaly');
    await page.getByLabel(/birth date/i).fill(adultBirthDate);
    await page.getByLabel(/mobile number/i).fill('+201006158123');
    await page.getByLabel(/email/i).fill('mohammed@example.com');

    await page.getByLabel(/governorate/i).selectOption('1');
    await expect(page.getByRole('option', { name: 'Nasr City' })).toBeAttached();
    await page.getByLabel(/city/i).selectOption('101');

    await page.getByLabel(/street/i).fill('Abbas El Akkad');
    await page.getByLabel(/building number/i).fill('12A');
    await page.getByLabel(/flat number/i).fill('3');

    const submit = page.getByRole('button', { name: /submit registration/i });
    await expect(submit).toBeEnabled();
    await submit.click();

    await expect(page.getByText(/registration created/i)).toBeVisible();
  });

  test('validation failure path: invalid name blocks submit and shows an error', async ({ page }) => {
    await page.getByLabel(/first name/i).fill('Mohammed123');
    await page.getByLabel(/last name/i).fill('Ghaly');
    await page.getByLabel(/birth date/i).fill(adultBirthDate);
    await page.getByLabel(/email/i).fill('mohammed@example.com');

    // the first-name error should be shown and submit should stay disabled.
    await expect(page.getByText(/only contain Arabic or English letters/i).first()).toBeVisible();
    await expect(page.getByRole('button', { name: /submit registration/i })).toBeDisabled();
  });
});
