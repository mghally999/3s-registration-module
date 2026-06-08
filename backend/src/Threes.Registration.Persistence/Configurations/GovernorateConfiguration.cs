using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threes.Registration.Domain.Lookups;
using Threes.Registration.Persistence.Seeding;

namespace Threes.Registration.Persistence.Configurations;

// governorate lookup table plus its seed data. ids are fixed so the seed is
// stable across migrations and the city foreign keys line up.
public sealed class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.IsActive).IsRequired();

        builder.HasMany(g => g.Cities)
            .WithOne(c => c.Governorate!)
            .HasForeignKey(c => c.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(LookupSeedData.Governorates);
    }
}
