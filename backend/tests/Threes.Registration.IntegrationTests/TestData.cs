namespace Threes.Registration.IntegrationTests;

// builds valid request payloads with unique email + mobile so tests sharing the
// one container database never collide on the unique indexes.
public static class TestData
{
    private static int _counter;

    public static object ValidRequest(
        string? email = null,
        string? mobile = null,
        int governorateId = 1,
        int cityId = 101)
    {
        var seq = Interlocked.Increment(ref _counter);

        return new
        {
            firstName = "Mohammed",
            middleName = "Ahmed",
            lastName = "Ghaly",
            birthDate = "1995-04-12",
            // valid egyptian mobiles all start with +2010..; varying the tail
            // keeps them valid and unique.
            mobileNumber = mobile ?? $"+20100615{seq:D4}",
            email = email ?? $"user{seq}@example.com",
            addresses = new[]
            {
                new
                {
                    governorateId,
                    cityId,
                    street = "Abbas El Akkad",
                    buildingNumber = "12A",
                    flatNumber = "10/2",
                    isPrimary = true,
                },
            },
        };
    }
}

// shape of the create response.
public sealed record CreatedRegistration(Guid Id);
