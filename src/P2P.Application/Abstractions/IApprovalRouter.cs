using P2P.Domain.Requisitions;

namespace P2P.Application.Abstractions;

public interface IApprovalRouter
{
    Task<IReadOnlyList<ApprovalRouteStep>> ResolveRouteAsync(
        Guid costCentreId,
        decimal amount,
        string currency,
        CancellationToken ct = default);
}
