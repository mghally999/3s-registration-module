namespace Threes.Registration.IntegrationTests;

// lightweight mirrors of the api responses, just enough to assert on. kept
// separate from the application dtos so the tests treat the api as a black box.
public sealed record RegistrationDetailsResponse(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string MobileNumber,
    string Email,
    List<AddressResponse> Addresses);

public sealed record AddressResponse(
    Guid Id,
    int GovernorateId,
    string GovernorateName,
    int CityId,
    string CityName,
    string Street,
    string BuildingNumber,
    string FlatNumber,
    bool IsPrimary);

public sealed record GovernorateResponse(int Id, string Name);

public sealed record CityResponse(int Id, int GovernorateId, string Name);

public sealed record ValidationProblemResponse(Dictionary<string, string[]> Errors);

public sealed record PagedResultResponse<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public sealed record RegistrationSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    string MobileNumber,
    DateTimeOffset CreatedAtUtc,
    int AddressCount);
