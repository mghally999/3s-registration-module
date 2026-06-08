// the shapes the backend speaks. these line up with the api dtos one for one.

export interface Governorate {
  id: number;
  name: string;
}

export interface City {
  id: number;
  governorateId: number;
  name: string;
}

export interface AddressPayload {
  governorateId: number;
  cityId: number;
  street: string;
  buildingNumber: string;
  flatNumber: string;
  isPrimary: boolean;
}

export interface CreateRegistrationPayload {
  firstName: string;
  middleName?: string | null;
  lastName: string;
  birthDate: string; // yyyy-mm-dd
  mobileNumber: string;
  email: string;
  addresses: AddressPayload[];
}

export interface CreateRegistrationResult {
  id: string;
}

// rfc7807 problem details. validation errors arrive under `errors`, keyed by
// field; a 409 conflict carries a `field` so we can target the right input.
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  field?: string;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails | null,
  ) {
    super(problem?.detail ?? problem?.title ?? `request failed with status ${status}`);
    this.name = 'ApiError';
  }
}
