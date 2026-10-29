using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Requisitions;

namespace P2P.Infrastructure.Persistence.Configurations;

internal sealed class RequisitionConfiguration
    : IEntityTypeConfiguration<Requisition>
{
    public void Configure(EntityTypeBuilder<Requisition> builder)
    {
        builder.ToTable("Requisitions", "procurement");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Number).HasMaxLength(30).IsRequired();
        builder.HasIndex(r => r.Number).IsUnique()
            .HasDatabaseName("UX_Requisitions_Number");

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Justification).HasMaxLength(2000);
        builder.Property(r => r.Currency).HasMaxLength(3).IsRequired();
        builder.Property(r => r.RejectionReason).HasMaxLength(1000);

        builder.Property(r => r.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasMany(r => r.Lines)
            .WithOne()
            .HasForeignKey(l => l.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_lines");

        builder.HasMany(r => r.ApprovalSteps)
            .WithOne()
            .HasForeignKey(s => s.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.ApprovalSteps)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_approvalSteps");

        builder.Ignore(r => r.DomainEvents);
        builder.Ignore(r => r.Total);
        builder.Ignore(r => r.IsEditable);

        // Supports the approval inbox and "my requisitions" queries.
        builder.HasIndex(r => new { r.Status, r.SubmittedAtUtc })
            .HasDatabaseName("IX_Requisitions_Status_Submitted");

        builder.HasIndex(r => new { r.RequesterId, r.Status })
            .HasDatabaseName("IX_Requisitions_Requester_Status");
    }
}

internal sealed class RequisitionLineConfiguration
    : IEntityTypeConfiguration<RequisitionLine>
{
    public void Configure(EntityTypeBuilder<RequisitionLine> builder)
    {
        builder.ToTable("RequisitionLines", "procurement");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.ItemCode).HasMaxLength(50);
        builder.Property(l => l.Description).HasMaxLength(500).IsRequired();
        builder.Property(l => l.UnitOfMeasure).HasMaxLength(10).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 4);

        builder.ComplexProperty(l => l.UnitPrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("UnitPrice").HasPrecision(18, 4);
            price.Property(p => p.Currency)
                .HasColumnName("UnitPriceCurrency").HasMaxLength(3);
        });

        builder.Ignore(l => l.LineTotal);

        builder.HasIndex(l => l.RequisitionId)
            .HasDatabaseName("IX_RequisitionLines_Requisition");
    }
}

internal sealed class ApprovalStepConfiguration
    : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps", "procurement");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.RequiredRole).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Comment).HasMaxLength(1000);

        builder.Property(s => s.Status)
            .HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(s => new { s.RequisitionId, s.Sequence })
            .IsUnique()
            .HasDatabaseName("UX_ApprovalSteps_Requisition_Sequence");

        // Drives the approval inbox lookup.
        builder.HasIndex(s => new { s.Status, s.RequiredRole })
            .HasDatabaseName("IX_ApprovalSteps_Status_Role");
    }
}
