using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Vendors;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors", "procurement");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(v => v.Code)
            .IsUnique()
            .HasDatabaseName("UX_Vendors_Code");

        builder.Property(v => v.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(v => v.TradingName).HasMaxLength(200);
        builder.Property(v => v.TaxIdentifier).HasMaxLength(50);
        builder.Property(v => v.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.Property(v => v.HoldReason).HasMaxLength(1000);

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.ComplexProperty(v => v.RemitToAddress, address =>
        {
            address.Property(a => a.Line1)
                .HasColumnName("RemitToLine1").HasMaxLength(200).IsRequired();
            address.Property(a => a.Line2)
                .HasColumnName("RemitToLine2").HasMaxLength(200);
            address.Property(a => a.City)
                .HasColumnName("RemitToCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.StateOrProvince)
                .HasColumnName("RemitToState").HasMaxLength(100);
            address.Property(a => a.PostalCode)
                .HasColumnName("RemitToPostalCode").HasMaxLength(20).IsRequired();
            address.Property(a => a.CountryCode)
                .HasColumnName("RemitToCountryCode").HasMaxLength(2).IsRequired();
        });

        builder.HasMany(v => v.BankAccounts)
            .WithOne()
            .HasForeignKey(b => b.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.BankAccounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_bankAccounts");

        builder.Ignore(v => v.DomainEvents);
        builder.Ignore(v => v.CanReceivePurchaseOrders);

        builder.HasIndex(v => v.Status).HasDatabaseName("IX_Vendors_Status");
    }
}
