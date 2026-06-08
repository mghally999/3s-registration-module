namespace Threes.Registration.Domain.Lookups;

// reference data: a city that belongs to exactly one governorate. the
// GovernorateId link is what lets us both filter the city dropdown and reject
// a city that does not belong to the chosen governorate.
public sealed class City
{
    private City()
    {
    }

    public City(int id, int governorateId, string name, bool isActive = true)
    {
        Id = id;
        GovernorateId = governorateId;
        Name = name;
        IsActive = isActive;
    }

    public int Id { get; private set; }
    public int GovernorateId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public Governorate? Governorate { get; private set; }
}
