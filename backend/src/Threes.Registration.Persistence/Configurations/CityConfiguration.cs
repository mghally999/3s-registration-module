using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threes.Registration.Domain.Lookups;
using Threes.Registration.Persistence.Seeding;

namespace Threes.Registration.Persistence.Configurations;

// city lookup table plus its seed data. each city carries the governorate it
// belongs to, which is the link that powers both the dependent dropdown and
// the "city belongs to governorate" validation.
public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.GovernorateId).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();

        // index the foreign key so "cities for governorate X" is a fast seek.
        builder.HasIndex(c => c.GovernorateId);

        builder.HasData(LookupSeedData.Cities);
    }
}
