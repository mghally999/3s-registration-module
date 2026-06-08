using Microsoft.Extensions.DependencyInjection;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Lookups.Contracts;
using Threes.Registration.Infrastructure.Lookups;

namespace Threes.Registration.UnitTests.TestSupport;

// a frozen clock so age and timestamp logic is deterministic in tests.
public sealed class FixedClock : IDateTimeProvider
{
    public FixedClock(DateOnly today)
    {
        Today = today;
        UtcNow = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    public DateTimeOffset UtcNow { get; }
    public DateOnly Today { get; }
}

// an in-memory lookup store seeded with a tiny fixed set, used to drive the
// real LookupCache without a database.
public sealed class FakeLookupReadStore : ILookupReadStore
{
    private readonly IReadOnlyList<GovernorateDto> _governorates;
    private readonly IReadOnlyList<CityDto> _cities;

    public FakeLookupReadStore()
    {
        _governorates = new List<GovernorateDto>
        {
            new(1, "Cairo"),
            new(2, "Giza"),
        };

        _cities = new List<CityDto>
        {
            new(101, 1, "Nasr City"),
            new(102, 1, "Maadi"),
            new(201, 2, "Dokki"),
            new(203, 2, "6th of October"),
        };
    }

    public Task<IReadOnlyList<GovernorateDto>> GetActiveGovernoratesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_governorates);

    public Task<IReadOnlyList<CityDto>> GetActiveCitiesAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_cities);
}

// builds a real LookupCache over the fake store. the cache resolves the store
// from a scope, so we hand it a real service-scope-factory.
public static class TestLookupCache
{
    public static ILookupCache Create()
    {
        var services = new ServiceCollection();
        services.AddScoped<ILookupReadStore, FakeLookupReadStore>();
        var provider = services.BuildServiceProvider();
        return new LookupCache(provider.GetRequiredService<IServiceScopeFactory>());
    }
}
