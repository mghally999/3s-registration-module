import {
  ApiError,
  City,
  CreateRegistrationPayload,
  CreateRegistrationResult,
  Governorate,
  PagedResult,
  ProblemDetails,
  RegistrationDetail,
  RegistrationSummary,
} from './types';

// base url is empty in dev (vite proxies /api) and can be set per environment.
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? '';

async function parseProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return null;
  }
}

async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: { Accept: 'application/json' },
  });

  if (!response.ok) {
    throw new ApiError(response.status, await parseProblem(response));
  }

  return (await response.json()) as T;
}

export const api = {
  getGovernorates: () => getJson<Governorate[]>('/api/lookups/governorates'),

  getCities: (governorateId: number) =>
    getJson<City[]>(`/api/lookups/cities?governorateId=${governorateId}`),

  searchRegistrations: (page: number, pageSize: number, search: string) =>
    getJson<PagedResult<RegistrationSummary>>(
      `/api/registrations?page=${page}&pageSize=${pageSize}` +
        (search ? `&search=${encodeURIComponent(search)}` : ''),
    ),

  getRegistration: (id: string) => getJson<RegistrationDetail>(`/api/registrations/${id}`),

  async createRegistration(payload: CreateRegistrationPayload): Promise<CreateRegistrationResult> {
    const response = await fetch(`${baseUrl}/api/registrations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      // 400 (validation) and 409 (duplicate) both come back as problem details
      // and are surfaced to the form so it can map them onto fields.
      throw new ApiError(response.status, await parseProblem(response));
    }

    return (await response.json()) as CreateRegistrationResult;
  },
};
