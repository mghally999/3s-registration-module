using FluentAssertions;
using Moq;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Models;
using Threes.Registration.Application.Registrations.Queries.SearchRegistrations;
using Xunit;

namespace Threes.Registration.UnitTests.Application;

public class SearchRegistrationsHandlerTests
{
    private readonly Mock<IRegistrationRepository> _repository = new();

    private SearchRegistrationsQueryHandler BuildHandler() => new(_repository.Object);

    private void SetupRepo() =>
        _repository
            .Setup(r => r.SearchAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int page, int size, string? _, CancellationToken __) =>
                new PagedResult<RegistrationSummaryDto>(Array.Empty<RegistrationSummaryDto>(), page, size, 0));

    [Fact]
    public async Task Clamps_a_zero_or_negative_page_to_one()
    {
        SetupRepo();
        var result = await BuildHandler().Handle(new SearchRegistrationsQuery(Page: 0), default);
        result.Page.Should().Be(1);
        _repository.Verify(r => r.SearchAsync(1, It.IsAny<int>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Caps_an_oversized_page_size_at_100()
    {
        SetupRepo();
        var result = await BuildHandler().Handle(new SearchRegistrationsQuery(Page: 1, PageSize: 5000), default);
        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task Falls_back_to_default_page_size_for_non_positive_size()
    {
        SetupRepo();
        await BuildHandler().Handle(new SearchRegistrationsQuery(Page: 1, PageSize: 0), default);
        _repository.Verify(r => r.SearchAsync(1, 20, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
