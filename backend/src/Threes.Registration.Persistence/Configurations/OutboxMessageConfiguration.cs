using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threes.Registration.Persistence.Outbox;

namespace Threes.Registration.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.OccurredOnUtc).IsRequired();
        builder.Property(m => m.ProcessedOnUtc);
        builder.Property(m => m.Error);

        // the processor only ever asks for "not yet processed, oldest first",
        // so index the processed flag to keep that query a cheap seek.
        builder.HasIndex(m => m.ProcessedOnUtc);
    }
}
