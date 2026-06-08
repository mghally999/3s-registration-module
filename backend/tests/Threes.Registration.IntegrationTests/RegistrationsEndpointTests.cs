using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Threes.Registration.IntegrationTests;

[Collection(nameof(RegistrationApiCollection))]
public class RegistrationsEndpointTests
{
    private readonly HttpClient _client;

    public RegistrationsEndpointTests(RegistrationApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Create_then_get_round_trips_the_registration()
    {
        var create = await _client.PostAsJsonAsync("/api/registrations", TestData.ValidRequest());

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        create.Headers.Location.Should().NotBeNull();

        var created = await create.Content.ReadFromJsonAsync<CreatedRegistration>();
        created!.Id.Should().NotBeEmpty();

        var get = await _client.GetAsync($"/api/registrations/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await get.Content.ReadFromJsonAsync<RegistrationDetailsResponse>();
        body!.FirstName.Should().Be("Mohammed");
        body.Addresses.Should().ContainSingle();
        body.Addresses[0].IsPrimary.Should().BeTrue();
        // governorate/city display names are resolved from the lookup cache.
        body.Addresses[0].GovernorateName.Should().Be("Cairo");
        body.Addresses[0].CityName.Should().Be("Nasr City");
    }

    [Fact]
    public async Task Duplicate_email_is_rejected_with_409()
    {
        var first = TestData.ValidRequest(email: "dupe-email@example.com");
        (await _client.PostAsJsonAsync("/api/registrations", first))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        // same email, different mobile -> conflict on email.
        var second = TestData.ValidRequest(email: "dupe-email@example.com");
        var response = await _client.PostAsJsonAsync("/api/registrations", second);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Duplicate_mobile_is_rejected_with_409()
    {
        var first = TestData.ValidRequest(mobile: "+201006159999");
        (await _client.PostAsJsonAsync("/api/registrations", first))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var second = TestData.ValidRequest(mobile: "+201006159999");
        var response = await _client.PostAsJsonAsync("/api/registrations", second);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task City_under_the_wrong_governorate_is_rejected_with_400()
    {
        // nasr city (101) is in cairo (1), not giza (2).
        var request = TestData.ValidRequest(governorateId: 2, cityId: 101);

        var response = await _client.PostAsJsonAsync("/api/registrations", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invalid_field_is_rejected_with_400_problem_details()
    {
        var request = new
        {
            firstName = "Mohammed123", // digits not allowed
            lastName = "Ghaly",
            birthDate = "1995-04-12",
            mobileNumber = "+201006158123",
            email = "ok@example.com",
            addresses = new[]
            {
                new { governorateId = 1, cityId = 101, street = "S", buildingNumber = "1", flatNumber = "2", isPrimary = true },
            },
        };

        var response = await _client.PostAsJsonAsync("/api/registrations", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        problem!.Errors.Keys.Should().Contain(k => k.Contains("FirstName"));
    }

    [Fact]
    public async Task Unknown_id_returns_404()
    {
        var response = await _client.GetAsync($"/api/registrations/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_endpoint_pages_and_searches_by_email()
    {
        // create a registration with a known, unique email then find it via the
        // paged search endpoint.
        var email = "searchable.user@example.com";
        (await _client.PostAsJsonAsync("/api/registrations", TestData.ValidRequest(email: email)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var page = await _client.GetFromJsonAsync<PagedResultResponse<RegistrationSummaryResponse>>(
            "/api/registrations?page=1&pageSize=10&search=searchable.user");

        page.Should().NotBeNull();
        page!.Items.Should().Contain(r => r.Email == email);
        page.Items.Should().OnlyContain(r => r.Email.Contains("searchable.user"));
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(10);
    }
}
