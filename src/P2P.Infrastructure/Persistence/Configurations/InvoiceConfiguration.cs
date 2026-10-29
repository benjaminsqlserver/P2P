using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Invoices;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", "payables");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.InternalNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(i => i.InternalNumber).IsUnique()
            .HasDatabaseName("UX_Invoices_InternalNumber");

        builder.Property(i => i.VendorInvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.ConfigureMoney(i => i.AmountPaid, "AmountPaid");

        builder.HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_lines");

        builder.HasMany(i => i.MatchExceptions)
            .WithOne()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.MatchExceptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_matchExceptions");

        builder.Ignore(i => i.DomainEvents);
        builder.Ignore(i => i.NetTotal);
        builder.Ignore(i => i.TaxTotal);
        builder.Ignore(i => i.GrossTotal);
        builder.Ignore(i => i.AmountOutstanding);
        builder.Ignore(i => i.HasUnresolvedExceptions);
        builder.Ignore(i => i.IsPayable);

        // Not unique: a duplicate must be recorded so the match can flag it.
        builder.HasIndex(i => new { i.VendorId, i.VendorInvoiceNumber })
            .HasDatabaseName("IX_Invoices_Vendor_VendorInvoiceNumber");

        builder.HasIndex(i => new { i.VendorId, i.Status })
            .HasDatabaseName("IX_Invoices_Vendor_Status");

        builder.HasIndex(i => i.PurchaseOrderId)
            .HasDatabaseName("IX_Invoices_PurchaseOrder");
    }
}

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines", "payables");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();

        builder.ConfigureMoney(l => l.UnitPrice, "UnitPrice");

        builder.Ignore(l => l.NetTotal);
        builder.Ignore(l => l.TaxTotal);
        builder.Ignore(l => l.GrossTotal);

        builder.HasIndex(l => l.InvoiceId)
            .HasDatabaseName("IX_InvoiceLines_Invoice");
    }
}

internal sealed class MatchExceptionConfiguration
    : IEntityTypeConfiguration<MatchException>
{
    public void Configure(EntityTypeBuilder<MatchException> builder)
    {
        builder.ToTable("MatchExceptions", "payables");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Type)
            .HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Resolution).HasMaxLength(1000);

        builder.Ignore(e => e.Variance);

        builder.HasIndex(e => e.InvoiceId)
            .HasDatabaseName("IX_MatchExceptions_Invoice");
    }
}
