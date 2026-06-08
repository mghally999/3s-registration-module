using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threes.Registration.Domain.Lookups;
using Threes.Registration.Domain.Registrations;
using Threes.Registration.Domain.ValueObjects;

namespace Threes.Registration.Persistence.Configurations;

// mapping for an address. it carries plain governorate/city ids with real
// foreign keys to the lookup tables so the database itself refuses an address
// that points at a city or governorate that does not exist.
public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Street)
            .HasConversion(v => v.Value, s => Street.FromPersisted(s))
            .HasColumnName("Street")
            .HasMaxLength(Street.MaxLength)
            .IsRequired();

        builder.Property(a => a.BuildingNumber)
            .HasConversion(v => v.Value, s => BuildingNumber.FromPersisted(s))
            .HasColumnName("BuildingNumber")
            .HasMaxLength(BuildingNumber.MaxLength)
            .IsRequired();

        builder.Property(a => a.FlatNumber)
            .HasConversion(v => v.Value, s => FlatNumber.FromPersisted(s))
            .HasColumnName("FlatNumber")
            .HasMaxLength(FlatNumber.MaxLength)
            .IsRequired();

        builder.Property(a => a.IsPrimary).IsRequired();
        builder.Property(a => a.GovernorateId).IsRequired();
        builder.Property(a => a.CityId).IsRequired();
        builder.Property(a => a.RegistrationId).IsRequired();

        // restrict on delete so you cannot remove a governorate/city that is
        // still referenced by an address.
        builder.HasOne<Governorate>()
            .WithMany()
            .HasForeignKey(a => a.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.RegistrationId);

        builder.Ignore(a => a.DomainEvents);
    }
}
