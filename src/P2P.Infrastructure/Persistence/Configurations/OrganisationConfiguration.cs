using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Organisation;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class CostCentreConfiguration : IEntityTypeConfiguration<CostCentre>
{
    public void Configure(EntityTypeBuilder<CostCentre> builder)
    {
        builder.ToTable("CostCentres", "procurement");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique()
            .HasDatabaseName("UX_CostCentres_Code");

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();

        builder.Ignore(c => c.DomainEvents);
    }
}

internal sealed class ApprovalPolicyConfiguration
    : IEntityTypeConfiguration<ApprovalPolicy>
{
    public void Configure(EntityTypeBuilder<ApprovalPolicy> builder)
    {
        builder.ToTable("ApprovalPolicies", "procurement");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.RequiredRole).HasMaxLength(50).IsRequired();

        builder.Ignore(p => p.DomainEvents);

        // Drives ApprovalRouter.ResolveRouteAsync.
        builder.HasIndex(p => new { p.IsActive, p.Currency, p.MinimumAmount })
            .HasDatabaseName("IX_ApprovalPolicies_Active_Currency_Amount");
    }
}
