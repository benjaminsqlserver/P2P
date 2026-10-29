using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.PurchaseOrders;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class PurchaseOrderConfiguration
    : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders", "procurement");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Number).HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.Number).IsUnique()
            .HasDatabaseName("UX_PurchaseOrders_Number");

        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.CancellationReason).HasMaxLength(1000);

        builder.Property(p => p.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.ComplexProperty(p => p.ShipToAddress, address =>
        {
            address.Property(a => a.Line1)
                .HasColumnName("ShipToLine1").HasMaxLength(200).IsRequired();
            address.Property(a => a.Line2)
                .HasColumnName("ShipToLine2").HasMaxLength(200);
            address.Property(a => a.City)
                .HasColumnName("ShipToCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.StateOrProvince)
                .HasColumnName("ShipToState").HasMaxLength(100);
            address.Property(a => a.PostalCode)
                .HasColumnName("ShipToPostalCode").HasMaxLength(20).IsRequired();
            address.Property(a => a.CountryCode)
                .HasColumnName("ShipToCountryCode").HasMaxLength(2).IsRequired();
        });

        builder.HasMany(p => p.Lines)
            .WithOne()
            .HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_lines");

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.NetTotal);
        builder.Ignore(p => p.TaxTotal);
        builder.Ignore(p => p.GrossTotal);
        builder.Ignore(p => p.IsReceivable);

        // Supports the vendor register's open-PO count.
        builder.HasIndex(p => new { p.VendorId, p.Status })
            .HasDatabaseName("IX_PurchaseOrders_Vendor_Status");

        builder.HasIndex(p => p.RequisitionId)
            .HasDatabaseName("IX_PurchaseOrders_Requisition");
    }
}

internal sealed class PurchaseOrderLineConfiguration
    : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines", "procurement");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.ItemCode).HasMaxLength(50);
        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();
        builder.Property(l => l.UnitOfMeasure).HasMaxLength(10).IsRequired();

        builder.ConfigureMoney(l => l.UnitPrice, "UnitPrice");

        builder.Ignore(l => l.QuantityOutstanding);
        builder.Ignore(l => l.QuantityAwaitingInvoice);
        builder.Ignore(l => l.NetTotal);
        builder.Ignore(l => l.TaxTotal);
        builder.Ignore(l => l.GrossTotal);
        builder.Ignore(l => l.IsFullyReceived);

        builder.HasIndex(l => l.PurchaseOrderId)
            .HasDatabaseName("IX_PurchaseOrderLines_PurchaseOrder");
    }
}
