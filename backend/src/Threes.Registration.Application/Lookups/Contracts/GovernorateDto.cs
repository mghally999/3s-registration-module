namespace Threes.Registration.Application.Lookups.Contracts;

// what the GET /api/lookups/governorates endpoint returns per row.
public sealed record GovernorateDto(int Id, string Name);
