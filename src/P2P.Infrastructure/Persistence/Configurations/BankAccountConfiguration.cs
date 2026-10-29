using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Vendors;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts", "procurement");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.BankName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.AccountName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(b => b.SortCodeOrSwift).HasMaxLength(20);
        builder.Property(b => b.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(b => b.VendorId)
            .HasDatabaseName("IX_BankAccounts_Vendor");
    }
}
