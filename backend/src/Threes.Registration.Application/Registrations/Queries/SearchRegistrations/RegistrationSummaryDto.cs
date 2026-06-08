namespace Threes.Registration.Application.Registrations.Queries.SearchRegistrations;

// a lightweight row for the registrations list/search endpoint. no addresses,
// just the count, so the list query stays cheap.
public sealed record RegistrationSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string MobileNumber,
    DateTimeOffset CreatedAtUtc,
    int AddressCount);
