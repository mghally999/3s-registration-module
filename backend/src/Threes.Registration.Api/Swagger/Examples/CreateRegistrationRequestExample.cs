using Swashbuckle.AspNetCore.Filters;
using Threes.Registration.Application.Registrations.Commands.CreateRegistration;

namespace Threes.Registration.Api.Swagger.Examples;

// the example request body swagger shows for POST /api/registrations. uses real
// seeded lookup ids (cairo = 1, nasr city = 101) so you can copy, paste and it
// actually works.
public sealed class CreateRegistrationRequestExample : IExamplesProvider<CreateRegistrationCommand>
{
    public CreateRegistrationCommand GetExamples() => new()
    {
        FirstName = "Mohammed",
        MiddleName = "Ahmed",
        LastName = "Ghaly",
        BirthDate = new DateOnly(1995, 4, 12),
        MobileNumber = "+201006158123",
        Email = "mohammed.ghaly@example.com",
        Addresses = new List<CreateAddressDto>
        {
            new()
            {
                GovernorateId = 1,
                CityId = 101,
                Street = "Abbas El Akkad",
                BuildingNumber = "12A",
                FlatNumber = "10/2",
                IsPrimary = true,
            },
            new()
            {
                GovernorateId = 2,
                CityId = 203,
                Street = "Central Axis",
                BuildingNumber = "5",
                FlatNumber = "3",
                IsPrimary = false,
            },
        },
    };
}
