using P2P.Domain.Common;

namespace P2P.Domain.PurchaseOrders;

public sealed record PurchaseOrderCreated(
    Guid PurchaseOrderId, string Number, Guid VendorId, Guid? RequisitionId)
    : DomainEvent;

public sealed record PurchaseOrderIssued(
    Guid PurchaseOrderId, string Number, Guid VendorId,
    decimal GrossTotal, string Currency) : DomainEvent;

public sealed record PurchaseOrderReceiptApplied(
    Guid PurchaseOrderId, string Number, PurchaseOrderStatus NewStatus) : DomainEvent;

public sealed record PurchaseOrderClosed(Guid PurchaseOrderId, string Number)
    : DomainEvent;

public sealed record PurchaseOrderCancelled(
    Guid PurchaseOrderId, string Number, string Reason) : DomainEvent;
