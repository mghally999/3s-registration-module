using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Threes.Registration.IntegrationTests;

[Collection(nameof(RegistrationApiCollection))]
public class LookupsEndpointTests
{
    private readonly HttpClient _client;

    public LookupsEndpointTests(RegistrationApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Governorates_returns_the_seeded_set_sorted()
    {
        var governorates = await _client.GetFromJsonAsync<List<GovernorateResponse>>("/api/lookups/governorates");

        governorates.Should().NotBeNull();
        governorates!.Should().HaveCountGreaterThanOrEqualTo(8);
        governorates!.Select(g => g.Name).Should().Contain(new[] { "Cairo", "Giza", "Alexandria" });
        // sorted by name.
        governorates!.Select(g => g.Name).Should().BeInAscendingOrder(StringComparer.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Cities_are_filtered_by_governorate()
    {
        var cairoCities = await _client.GetFromJsonAsync<List<CityResponse>>("/api/lookups/cities?governorateId=1");

        cairoCities.Should().NotBeNullOrEmpty();
        cairoCities!.Should().OnlyContain(c => c.GovernorateId == 1);
        cairoCities!.Select(c => c.Name).Should().Contain("Nasr City");
    }

    [Fact]
    public async Task Cities_for_unknown_governorate_is_empty()
    {
        var response = await _client.GetAsync("/api/lookups/cities?governorateId=999");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cities = await response.Content.ReadFromJsonAsync<List<CityResponse>>();
        cities.Should().BeEmpty();
    }
}
