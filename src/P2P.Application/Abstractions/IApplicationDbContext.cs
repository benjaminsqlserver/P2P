using Microsoft.EntityFrameworkCore;
using P2P.Domain.GoodsReceipts;
using P2P.Domain.Invoices;
using P2P.Domain.Organisation;
using P2P.Domain.Payments;
using P2P.Domain.PurchaseOrders;
using P2P.Domain.Requisitions;
using P2P.Domain.Vendors;

namespace P2P.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Vendor> Vendors { get; }
    DbSet<BankAccount> BankAccounts { get; }
    DbSet<Requisition> Requisitions { get; }
    DbSet<RequisitionLine> RequisitionLines { get; }
    DbSet<ApprovalStep> ApprovalSteps { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptLine> GoodsReceiptLines { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }
    DbSet<MatchException> MatchExceptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentAllocation> PaymentAllocations { get; }
    DbSet<CostCentre> CostCentres { get; }
    DbSet<ApprovalPolicy> ApprovalPolicies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
