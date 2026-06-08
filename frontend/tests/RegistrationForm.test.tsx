import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { RegistrationForm } from '../src/components/RegistrationForm';
import { ApiError } from '../src/api/types';

// the form talks to the api client; mock it so the component test stays fast
// and deterministic.
const getGovernorates = vi.fn();
const getCities = vi.fn();
const createRegistration = vi.fn();

vi.mock('../src/api/client', () => ({
  api: {
    getGovernorates: (...args: unknown[]) => getGovernorates(...args),
    getCities: (...args: unknown[]) => getCities(...args),
    createRegistration: (...args: unknown[]) => createRegistration(...args),
  },
}));

const adultBirthDate = `${new Date().getFullYear() - 25}-04-12`;

async function fillValidForm() {
  await userEvent.type(screen.getByLabelText(/first name/i), 'Mohammed');
  await userEvent.type(screen.getByLabelText(/last name/i), 'Ghaly');
  fireEvent.change(screen.getByLabelText(/birth date/i), { target: { value: adultBirthDate } });
  await userEvent.type(screen.getByLabelText(/mobile number/i), '+201006158123');
  await userEvent.type(screen.getByLabelText(/email/i), 'mohammed@example.com');

  // pick governorate -> cities load -> pick city.
  await userEvent.selectOptions(screen.getByLabelText(/governorate/i), '1');
  await waitFor(() => expect(getCities).toHaveBeenCalledWith(1));
  await screen.findByRole('option', { name: 'Nasr City' });
  await userEvent.selectOptions(screen.getByLabelText(/city/i), '101');

  await userEvent.type(screen.getByLabelText(/street/i), 'Abbas El Akkad');
  await userEvent.type(screen.getByLabelText(/building number/i), '12A');
  await userEvent.type(screen.getByLabelText(/flat number/i), '3');
}

describe('RegistrationForm', () => {
  beforeEach(() => {
    getGovernorates.mockResolvedValue([
      { id: 1, name: 'Cairo' },
      { id: 2, name: 'Giza' },
    ]);
    getCities.mockResolvedValue([
      { id: 101, governorateId: 1, name: 'Nasr City' },
      { id: 102, governorateId: 1, name: 'Maadi' },
    ]);
    createRegistration.mockReset();
  });

  it('disables submit until the form is valid', async () => {
    render(<RegistrationForm />);
    const submit = await screen.findByRole('button', { name: /submit registration/i });
    expect(submit).toBeDisabled();
  });

  it('loads cities only after a governorate is selected', async () => {
    render(<RegistrationForm />);
    await screen.findByRole('option', { name: 'Cairo' });

    const citySelect = screen.getByLabelText(/city/i);
    expect(citySelect).toBeDisabled();

    await userEvent.selectOptions(screen.getByLabelText(/governorate/i), '1');

    await waitFor(() => expect(citySelect).not.toBeDisabled());
    expect(screen.getByRole('option', { name: 'Nasr City' })).toBeInTheDocument();
  });

  it('submits a valid registration and shows the created id', async () => {
    createRegistration.mockResolvedValue({ id: '11111111-2222-3333-4444-555555555555' });
    render(<RegistrationForm />);
    await screen.findByRole('option', { name: 'Cairo' });

    await fillValidForm();

    const submit = screen.getByRole('button', { name: /submit registration/i });
    await waitFor(() => expect(submit).not.toBeDisabled());
    await userEvent.click(submit);

    await screen.findByText(/registration created/i);
    expect(createRegistration).toHaveBeenCalledTimes(1);
    const payload = createRegistration.mock.calls[0][0];
    expect(payload.addresses[0].governorateId).toBe(1);
    expect(payload.addresses[0].cityId).toBe(101);
  });

  it('maps a duplicate-email conflict onto the email field', async () => {
    createRegistration.mockRejectedValue(
      new ApiError(409, { status: 409, field: 'email', detail: 'A registration with this email already exists.' }),
    );
    render(<RegistrationForm />);
    await screen.findByRole('option', { name: 'Cairo' });

    await fillValidForm();

    const submit = screen.getByRole('button', { name: /submit registration/i });
    await waitFor(() => expect(submit).not.toBeDisabled());
    await userEvent.click(submit);

    const emailField = screen.getByLabelText(/email/i);
    await waitFor(() => expect(emailField).toHaveAttribute('aria-invalid', 'true'));
    expect(within(emailField.closest('.field') as HTMLElement).getByRole('alert')).toHaveTextContent(
      /already exists/i,
    );
  });
});
