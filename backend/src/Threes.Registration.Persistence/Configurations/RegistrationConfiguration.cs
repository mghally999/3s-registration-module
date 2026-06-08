using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threes.Registration.Domain.ValueObjects;

namespace Threes.Registration.Persistence.Configurations;

// fluent api mapping for the registration aggregate. each value object is
// either value-converted to a single column or, for email (two columns),
// mapped as an owned type. the unique indexes for normalized email and mobile
// live here.
public sealed class RegistrationConfiguration : IEntityTypeConfiguration<RegistrationAggregate>
{
    public void Configure(EntityTypeBuilder<RegistrationAggregate> builder)
    {
        builder.ToTable("Registrations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.FirstName)
            .HasConversion(v => v.Value, s => PersonName.FromPersisted(s))
            .HasColumnName("FirstName")
            .HasMaxLength(PersonName.MaxLength)
            .IsRequired();

        builder.Property(r => r.MiddleName)
            .HasConversion(
                v => v == null ? null : v.Value,
                s => s == null ? null : PersonName.FromPersisted(s))
            .HasColumnName("MiddleName")
            .HasMaxLength(PersonName.MaxLength);

        builder.Property(r => r.LastName)
            .HasConversion(v => v.Value, s => PersonName.FromPersisted(s))
            .HasColumnName("LastName")
            .HasMaxLength(PersonName.MaxLength)
            .IsRequired();

        builder.Property(r => r.BirthDate)
            .HasConversion(v => v.Value, d => BirthDate.FromPersisted(d))
            .HasColumnName("BirthDate")
            .HasColumnType("date")
            .IsRequired();

        // mobile is mapped as an owned single-column type (rather than a value
        // conversion) so that its normalized Value is a real, queryable column.
        // that lets the uniqueness check filter on r.Mobile.Value in sql.
        builder.OwnsOne(r => r.Mobile, mobile =>
        {
            mobile.Property(m => m.Value)
                .HasColumnName("MobileNumber")
                .HasMaxLength(16)
                .IsRequired();

            // stored already-normalized (e.164), so a plain unique index gives
            // us case-free uniqueness for free.
            mobile.HasIndex(m => m.Value)
                .IsUnique()
                .HasDatabaseName("UX_Registrations_MobileNumber");
        });
        builder.Navigation(r => r.Mobile).IsRequired();

        // email is two columns: the original for display and the normalized
        // (lower-cased) value that the unique index and lookups run against.
        builder.OwnsOne(r => r.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(EmailAddress.MaxLength)
                .IsRequired();

            email.Property(e => e.Normalized)
                .HasColumnName("EmailNormalized")
                .HasMaxLength(EmailAddress.MaxLength)
                .IsRequired();

            email.HasIndex(e => e.Normalized)
                .IsUnique()
                .HasDatabaseName("UX_Registrations_EmailNormalized");
        });
        builder.Navigation(r => r.Email).IsRequired();

        // audit columns.
        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.UpdatedBy).HasMaxLength(100);

        // one-to-many to addresses, mapped through the private _addresses
        // backing field (an AddressBook). ef adds rows into it via ICollection.
        builder.HasMany(r => r.Addresses)
            .WithOne()
            .HasForeignKey(a => a.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(RegistrationAggregate.Addresses))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // domain events are an in-memory concern, never a column.
        builder.Ignore(r => r.DomainEvents);
    }
}
