using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Exceptions;
using Threes.Registration.Application.Registrations.Commands.CreateRegistration;
using Threes.Registration.Infrastructure.Phone;
using Threes.Registration.UnitTests.TestSupport;
using Xunit;

namespace Threes.Registration.UnitTests.Application;

public class CreateRegistrationHandlerTests
{
    private static readonly DateOnly Today = new(2026, 6, 7);

    private readonly Mock<IRegistrationRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateRegistrationCommandHandler BuildHandler() => new(
        _repository.Object,
        _unitOfWork.Object,
        new LibPhoneNumberNormalizer(),
        new FixedClock(Today),
        NullLogger<CreateRegistrationCommandHandler>.Instance);

    private static CreateRegistrationCommand ValidCommand() => new()
    {
        FirstName = "Mohammed",
        LastName = "Ghaly",
        BirthDate = new DateOnly(1995, 4, 12),
        MobileNumber = "+201006158123",
        Email = "mohammed@example.com",
        Addresses = new List<CreateAddressDto>
        {
            new() { GovernorateId = 1, CityId = 101, Street = "Abbas El Akkad", BuildingNumber = "12A", FlatNumber = "10/2", IsPrimary = true },
        },
    };

    [Fact]
    public async Task Happy_path_saves_and_returns_an_id()
    {
        _repository.Setup(r => r.ExistsByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(r => r.ExistsByMobileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await BuildHandler().Handle(ValidCommand(), default);

        result.Id.Should().NotBeEmpty();
        _repository.Verify(r => r.AddAsync(It.IsAny<RegistrationAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Duplicate_email_throws_conflict_and_does_not_save()
    {
        _repository.Setup(r => r.ExistsByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => BuildHandler().Handle(ValidCommand(), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.Field.Should().Be("email");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Duplicate_mobile_throws_conflict()
    {
        _repository.Setup(r => r.ExistsByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(r => r.ExistsByMobileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => BuildHandler().Handle(ValidCommand(), default);

        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.Field.Should().Be("mobileNumber");
    }
}
