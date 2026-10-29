using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries", "infrastructure");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.TableName).HasMaxLength(128).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(20).IsRequired();

        // Serialised row snapshots: the global 400-character string
        // convention would truncate them, so these are unbounded (-1).
        // Provider-neutral: nvarchar(max) on SQL Server, TEXT on SQLite.
        builder.Property(a => a.OldValues).HasMaxLength(-1);
        builder.Property(a => a.NewValues).HasMaxLength(-1);
        builder.Property(a => a.ChangedColumns).HasMaxLength(-1);

        builder.HasIndex(a => new { a.TableName, a.EntityId })
            .HasDatabaseName("IX_AuditEntries_Table_Entity");

        builder.HasIndex(a => a.OccurredAtUtc)
            .HasDatabaseName("IX_AuditEntries_OccurredAt");
    }
}

internal sealed class DocumentSequenceConfiguration
    : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences", "infrastructure");

        builder.HasKey(s => new { s.DocumentType, s.Year });

        builder.Property(s => s.DocumentType).HasMaxLength(10);
    }
}
