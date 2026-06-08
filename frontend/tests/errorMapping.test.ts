import { describe, expect, it } from 'vitest';
import { mapServerErrors, toFormPath } from '../src/api/errorMapping';

describe('toFormPath', () => {
  it('lower-cases simple fields', () => {
    expect(toFormPath('FirstName')).toBe('firstName');
    expect(toFormPath('Email')).toBe('email');
  });

  it('converts indexed nested fields to react-hook-form paths', () => {
    expect(toFormPath('Addresses[0].CityId')).toBe('addresses.0.cityId');
    expect(toFormPath('Addresses[2].BuildingNumber')).toBe('addresses.2.buildingNumber');
  });
});

describe('mapServerErrors', () => {
  it('flattens the validation errors map', () => {
    const mapped = mapServerErrors({
      status: 400,
      errors: {
        FirstName: ['First name is required.'],
        'Addresses[0].CityId': ['City does not exist or does not belong to the selected governorate.'],
      },
    });

    expect(mapped).toContainEqual({ path: 'firstName', message: 'First name is required.' });
    expect(mapped).toContainEqual({
      path: 'addresses.0.cityId',
      message: 'City does not exist or does not belong to the selected governorate.',
    });
  });

  it('maps a 409 conflict onto the reported field', () => {
    const mapped = mapServerErrors({
      status: 409,
      field: 'email',
      detail: 'A registration with this email already exists.',
    });

    expect(mapped).toEqual([
      { path: 'email', message: 'A registration with this email already exists.' },
    ]);
  });

  it('returns nothing for an empty problem', () => {
    expect(mapServerErrors(null)).toEqual([]);
    expect(mapServerErrors({ status: 500 })).toEqual([]);
  });
});
