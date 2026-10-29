using System.Reflection;
using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Domain.GoodsReceipts;
using P2P.Domain.Invoices;
using P2P.Domain.Organisation;
using P2P.Domain.Payments;
using P2P.Domain.PurchaseOrders;
using P2P.Domain.Requisitions;
using P2P.Domain.Vendors;

namespace P2P.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Requisition> Requisitions => Set<Requisition>();
    public DbSet<RequisitionLine> RequisitionLines => Set<RequisitionLine>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<MatchException> MatchExceptions => Set<MatchException>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<CostCentre> CostCentres => Set<CostCentre>();
    public DbSet<ApprovalPolicy> ApprovalPolicies => Set<ApprovalPolicy>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        // Every decimal in this application is money or a quantity.
        // Set a sane global precision so nobody has to remember.
        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 4);

        configurationBuilder.Properties<string>()
            .HaveMaxLength(400);

        base.ConfigureConventions(configurationBuilder);
    }
}
