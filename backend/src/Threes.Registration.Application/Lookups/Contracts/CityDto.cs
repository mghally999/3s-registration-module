namespace Threes.Registration.Application.Lookups.Contracts;

// what the GET /api/lookups/cities endpoint returns per row. GovernorateId is
// included so the frontend can keep its dependent dropdown honest.
public sealed record CityDto(int Id, int GovernorateId, string Name);
