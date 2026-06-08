using Microsoft.EntityFrameworkCore;
using Threes.Registration.Domain.Lookups;
using Threes.Registration.Domain.Registrations;
using Threes.Registration.Persistence.Outbox;

namespace Threes.Registration.Persistence;

// the one ef core context. it pulls in all the IEntityTypeConfiguration classes
// from this assembly so the mapping stays out of here and next to each entity.
public sealed class RegistrationDbContext : DbContext
{
    public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options)
    {
    }

    public DbSet<RegistrationAggregate> Registrations => Set<RegistrationAggregate>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegistrationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
