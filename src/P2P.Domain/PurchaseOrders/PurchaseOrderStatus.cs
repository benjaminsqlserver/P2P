namespace P2P.Domain.PurchaseOrders;

public enum PurchaseOrderStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyReceived = 2,
    Received = 3,
    Closed = 4,
    Cancelled = 5
}
