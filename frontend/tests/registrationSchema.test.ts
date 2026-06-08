import { describe, expect, it } from 'vitest';
import { ageOn, registrationSchema } from '../src/validation/registrationSchema';

// a birthday roughly 25 years ago so the age rule always passes.
const adultBirthDate = `${new Date().getFullYear() - 25}-04-12`;

const validInput = () => ({
  firstName: 'Mohammed',
  middleName: 'Ahmed',
  lastName: 'Ghaly',
  birthDate: adultBirthDate,
  mobileNumber: '+201006158123',
  email: 'mohammed@example.com',
  addresses: [
    {
      governorateId: 1,
      cityId: 101,
      street: 'Abbas El Akkad',
      buildingNumber: '12A',
      flatNumber: '10/2',
      isPrimary: true,
    },
  ],
});

describe('ageOn', () => {
  it('does not count a birthday that has not happened yet this year', () => {
    const today = new Date('2026-06-07T00:00:00');
    expect(ageOn('2000-06-08', today)).toBe(25);
    expect(ageOn('2000-06-07', today)).toBe(26);
    expect(ageOn('2000-01-01', today)).toBe(26);
  });
});

describe('registrationSchema', () => {
  it('accepts a fully valid registration', () => {
    expect(registrationSchema.safeParse(validInput()).success).toBe(true);
  });

  it('accepts arabic names', () => {
    const input = { ...validInput(), firstName: 'محمد', lastName: 'علي' };
    expect(registrationSchema.safeParse(input).success).toBe(true);
  });

  it('rejects names with digits', () => {
    const input = { ...validInput(), firstName: 'Mohammed2' };
    const result = registrationSchema.safeParse(input);
    expect(result.success).toBe(false);
  });

  it('rejects someone under 20', () => {
    const input = { ...validInput(), birthDate: `${new Date().getFullYear() - 10}-01-01` };
    expect(registrationSchema.safeParse(input).success).toBe(false);
  });

  it('rejects an invalid email', () => {
    const input = { ...validInput(), email: 'nope' };
    expect(registrationSchema.safeParse(input).success).toBe(false);
  });

  it('requires at least one address', () => {
    const input = { ...validInput(), addresses: [] };
    expect(registrationSchema.safeParse(input).success).toBe(false);
  });

  it('rejects more than five addresses', () => {
    const one = validInput().addresses[0];
    const input = { ...validInput(), addresses: Array.from({ length: 6 }, () => ({ ...one })) };
    expect(registrationSchema.safeParse(input).success).toBe(false);
  });

  it('rejects more than one primary address', () => {
    const one = validInput().addresses[0];
    const input = {
      ...validInput(),
      addresses: [
        { ...one, isPrimary: true },
        { ...one, cityId: 102, isPrimary: true },
      ],
    };
    expect(registrationSchema.safeParse(input).success).toBe(false);
  });

  it('rejects a building number with disallowed characters', () => {
    const input = validInput();
    input.addresses[0].buildingNumber = '12#A';
    expect(registrationSchema.safeParse(input).success).toBe(false);
  });
});
