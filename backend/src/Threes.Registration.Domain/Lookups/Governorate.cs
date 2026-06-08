namespace Threes.Registration.Domain.Lookups;

// reference data: an egyptian governorate. these are seeded and read-only from
// the app's point of view, so this is a plain lookup entity with an int id
// rather than a full aggregate.
public sealed class Governorate
{
    private readonly List<City> _cities = new();

    private Governorate()
    {
    }

    public Governorate(int id, string name, bool isActive = true)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<City> Cities => _cities;
}
