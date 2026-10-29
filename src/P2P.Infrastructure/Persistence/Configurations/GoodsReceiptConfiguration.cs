using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.GoodsReceipts;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class GoodsReceiptConfiguration
    : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts", "procurement");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.Number).HasMaxLength(30).IsRequired();
        builder.HasIndex(g => g.Number).IsUnique()
            .HasDatabaseName("UX_GoodsReceipts_Number");

        builder.Property(g => g.DeliveryNoteReference).HasMaxLength(100);
        builder.Property(g => g.Carrier).HasMaxLength(100);
        builder.Property(g => g.ReversalReason).HasMaxLength(1000);

        builder.HasMany(g => g.Lines)
            .WithOne()
            .HasForeignKey(l => l.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(g => g.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_lines");

        builder.Ignore(g => g.DomainEvents);

        builder.HasIndex(g => g.PurchaseOrderId)
            .HasDatabaseName("IX_GoodsReceipts_PurchaseOrder");
    }
}

internal sealed class GoodsReceiptLineConfiguration
    : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("GoodsReceiptLines", "procurement");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.RejectionReason).HasMaxLength(1000);
        builder.Property(l => l.Notes).HasMaxLength(1000);

        builder.Ignore(l => l.QuantityDelivered);

        builder.HasIndex(l => l.GoodsReceiptId)
            .HasDatabaseName("IX_GoodsReceiptLines_GoodsReceipt");
    }
}
