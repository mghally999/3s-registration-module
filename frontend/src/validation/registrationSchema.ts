import { z } from 'zod';

// these rules deliberately mirror the backend's domain/fluentvalidation rules.
// the backend re-validates everything, but matching here gives instant inline
// feedback and stops obviously bad submits.

// english + arabic letters, with single space / hyphen / apostrophe between
// letters. no leading/trailing/double separators.
const namePattern = /^[A-Za-zء-ي]+(?:[ '\-][A-Za-zء-ي]+)*$/;

// letters, digits, slash, dash, spaces (e.g. "12A", "10/2").
const buildingFlatPattern = /^[A-Za-z0-9 /-]+$/;

const MIN_AGE = 20;

export function normalizeName(value: string): string {
  return value.trim().replace(/\s+/g, ' ');
}

export function ageOn(birthDate: string, today: Date): number {
  const birth = new Date(`${birthDate}T00:00:00`);
  let age = today.getFullYear() - birth.getFullYear();
  const monthDiff = today.getMonth() - birth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age -= 1;
  }
  return age;
}

const nameField = (label: string) =>
  z
    .string()
    .transform(normalizeName)
    .refine((v) => v.length > 0, { message: `${label} is required.` })
    .refine((v) => v.length <= 50, { message: `${label} must be at most 50 characters.` })
    .refine((v) => namePattern.test(v), {
      message: `${label} may only contain Arabic or English letters, spaces, hyphen and apostrophe.`,
    });

const shortText = (label: string, max: number) =>
  z
    .string()
    .transform((v) => v.trim().replace(/\s+/g, ' '))
    .refine((v) => v.length > 0, { message: `${label} is required.` })
    .refine((v) => v.length <= max, { message: `${label} must be at most ${max} characters.` })
    .refine((v) => buildingFlatPattern.test(v), {
      message: `${label} may only contain letters, numbers, slash, dash and spaces.`,
    });

export const addressSchema = z.object({
  governorateId: z.coerce.number().int().positive('Governorate is required.'),
  cityId: z.coerce.number().int().positive('City is required.'),
  street: z
    .string()
    .transform((v) => v.trim())
    .refine((v) => v.length > 0, { message: 'Street is required.' })
    .refine((v) => v.length <= 200, { message: 'Street must be at most 200 characters.' }),
  buildingNumber: shortText('Building number', 20),
  flatNumber: shortText('Flat number', 20),
  isPrimary: z.boolean(),
});

export const registrationSchema = z
  .object({
    firstName: nameField('First name'),
    // optional: empty string is fine, otherwise it must pass the name rules.
    middleName: z
      .string()
      .transform(normalizeName)
      .refine((v) => v.length === 0 || (v.length <= 50 && namePattern.test(v)), {
        message: 'Middle name may only contain Arabic or English letters, spaces, hyphen and apostrophe.',
      })
      .optional(),
    lastName: nameField('Last name'),
    birthDate: z
      .string()
      .min(1, 'Birth date is required.')
      .refine((v) => new Date(`${v}T00:00:00`) <= new Date(), { message: 'Birth date cannot be in the future.' })
      .refine((v) => ageOn(v, new Date()) >= MIN_AGE, { message: `Minimum age is ${MIN_AGE} years.` }),
    mobileNumber: z
      .string()
      .min(1, 'Mobile number is required.')
      // permissive on the client (local or international); the backend
      // normalizes to e.164 and is the source of truth.
      .refine((v) => /^\+?\d[\d\s-]{6,}$/.test(v.trim()), { message: 'Enter a valid mobile number.' }),
    email: z
      .string()
      .min(1, 'Email is required.')
      .max(254, 'Email must be at most 254 characters.')
      .email('Email format is not valid.'),
    addresses: z
      .array(addressSchema)
      .min(1, 'At least one address is required.')
      .max(5, 'A registration can have at most 5 addresses.')
      .refine((list) => list.filter((a) => a.isPrimary).length <= 1, {
        message: 'Only one address can be marked as primary.',
      }),
  });

export type RegistrationFormValues = z.input<typeof registrationSchema>;
export type RegistrationParsed = z.output<typeof registrationSchema>;
