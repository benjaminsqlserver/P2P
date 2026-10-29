using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Application.Payments;
using P2P.Contracts.Dashboard;
using P2P.Domain.Common;
using P2P.Domain.Invoices;
using P2P.Domain.PurchaseOrders;
using P2P.Domain.Requisitions;

namespace P2P.Application.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery : IQuery<DashboardSummaryDto>;

internal sealed class GetDashboardSummaryQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private const int PayableHorizonDays = 7;

    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var roles = currentUser.Roles.ToArray();
        var userId = currentUser.UserId;

        // Same rule as the approval inbox, so the card and the inbox agree.
        var awaitingMyApproval = await (
            from r in context.Requisitions.AsNoTracking()
            where r.Status == RequisitionStatus.PendingApproval
                  && r.RequesterId != userId
            let pending = r.ApprovalSteps
                .Where(s => s.Status == ApprovalStepStatus.Pending)
                .OrderBy(s => s.Sequence)
                .FirstOrDefault()
            where pending != null
                  && (roles.Contains(pending.RequiredRole)
                      || pending.AssignedApproverId == userId)
            select r.Id)
            .CountAsync(ct);

        var openPurchaseOrders = await context.PurchaseOrders
            .AsNoTracking()
            .CountAsync(p => p.Status == PurchaseOrderStatus.Issued
                             || p.Status == PurchaseOrderStatus.PartiallyReceived
                             || p.Status == PurchaseOrderStatus.Received, ct);

        // Same rule as the match exception workbench queue.
        var unresolvedExceptions = await context.Invoices
            .AsNoTracking()
            .CountAsync(i => i.Status == InvoiceStatus.Exception
                             && i.MatchExceptions.Any(e => !e.IsResolved), ct);

        // Same rule as the payment proposal, with a one-week horizon.
        var horizon = clock.Today.AddDays(PayableHorizonDays);
        var alreadyAllocated = PaymentQueries.InvoicesOnLivePayments(context);

        var payable = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => (i.Status == InvoiceStatus.Approved
                         || i.Status == InvoiceStatus.PartiallyPaid)
                        && i.Currency == DashboardCurrency.Code
                        && i.DueDate <= horizon
                        && !alreadyAllocated.Contains(i.Id))
            .ToListAsync(ct);

        return new DashboardSummaryDto
        {
            AwaitingMyApproval = awaitingMyApproval,
            OpenPurchaseOrders = openPurchaseOrders,
            UnresolvedExceptions = unresolvedExceptions,
            PayableThisWeek = payable.Sum(i => i.AmountOutstanding.Amount)
        };
    }
}
