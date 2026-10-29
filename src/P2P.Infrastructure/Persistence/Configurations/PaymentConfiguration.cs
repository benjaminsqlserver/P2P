using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Payments;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "payables");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Number).HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.Number).IsUnique()
            .HasDatabaseName("UX_Payments_Number");

        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasMany(p => p.Allocations)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_allocations");

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.Total);

        builder.HasIndex(p => new { p.VendorId, p.Status })
            .HasDatabaseName("IX_Payments_Vendor_Status");
    }
}

internal sealed class PaymentAllocationConfiguration
    : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations", "payables");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.ConfigureMoney(a => a.Amount, "Amount");

        builder.HasIndex(a => a.PaymentId)
            .HasDatabaseName("IX_PaymentAllocations_Payment");

        builder.HasIndex(a => a.InvoiceId)
            .HasDatabaseName("IX_PaymentAllocations_Invoice");
    }
}
