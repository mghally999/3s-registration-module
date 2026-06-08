using FluentAssertions;
using Threes.Registration.UnitTests.TestSupport;
using Xunit;

namespace Threes.Registration.UnitTests.Application;

// exercises the cache (and through it the merge sort + hashmap + binary search)
// over the fake lookup store.
public class LookupCacheTests
{
    [Fact]
    public async Task Governorates_come_back_sorted_by_name()
    {
        var cache = TestLookupCache.Create();

        var governorates = await cache.GetGovernoratesAsync(default);

        governorates.Select(g => g.Name).Should().Equal("Cairo", "Giza");
    }

    [Fact]
    public async Task Cities_are_filtered_by_governorate_and_sorted_by_name()
    {
        var cache = TestLookupCache.Create();

        var cairoCities = await cache.GetCitiesAsync(1, default);

        cairoCities.Select(c => c.Name).Should().Equal("Maadi", "Nasr City");
        cairoCities.Should().OnlyContain(c => c.GovernorateId == 1);
    }

    [Fact]
    public async Task Unknown_governorate_returns_no_cities()
    {
        var cache = TestLookupCache.Create();
        (await cache.GetCitiesAsync(999, default)).Should().BeEmpty();
    }

    [Fact]
    public async Task GovernorateExists_reflects_the_seed()
    {
        var cache = TestLookupCache.Create();

        (await cache.GovernorateExistsAsync(1, default)).Should().BeTrue();
        (await cache.GovernorateExistsAsync(99, default)).Should().BeFalse();
    }

    [Theory]
    [InlineData(101, 1, true)]
    [InlineData(102, 1, true)]
    [InlineData(101, 2, false)] // nasr city is cairo, not giza
    [InlineData(999, 1, false)] // city does not exist at all
    public async Task CityBelongsToGovernorate_is_strict(int cityId, int governorateId, bool expected)
    {
        var cache = TestLookupCache.Create();

        var result = await cache.CityBelongsToGovernorateAsync(cityId, governorateId, default);

        result.Should().Be(expected);
    }
}
