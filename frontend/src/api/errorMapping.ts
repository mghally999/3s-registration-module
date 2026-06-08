import { ProblemDetails } from './types';

// the backend reports validation errors with pascal-case, indexed keys like
// "Addresses[0].CityId". react-hook-form addresses fields as "addresses.0.cityId".
// this converts one to the other so server errors light up the right input.
export function toFormPath(serverKey: string): string {
  return serverKey
    .replace(/\[(\d+)\]/g, '.$1') // Addresses[0] -> Addresses.0
    .split('.')
    .map((segment) => (/^\d+$/.test(segment) ? segment : lowerFirst(segment)))
    .join('.');
}

function lowerFirst(value: string): string {
  return value.length === 0 ? value : value[0].toLowerCase() + value.slice(1);
}

export interface MappedFieldError {
  path: string;
  message: string;
}

// flattens a problem-details payload into a list of (form path, message) pairs.
// handles both 400 validation (the `errors` map) and 409 conflict (the single
// `field` + `detail`).
export function mapServerErrors(problem: ProblemDetails | null): MappedFieldError[] {
  if (!problem) {
    return [];
  }

  if (problem.errors) {
    return Object.entries(problem.errors).flatMap(([key, messages]) =>
      messages.map((message) => ({ path: toFormPath(key), message })),
    );
  }

  if (problem.field) {
    return [{ path: toFormPath(problem.field), message: problem.detail ?? 'Conflict.' }];
  }

  return [];
}
