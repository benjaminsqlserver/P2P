using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Contracts.Common;
using P2P.Domain.Requisitions;
using P2P.Infrastructure.Persistence;

namespace P2P.Infrastructure.Services;

internal sealed class ApprovalRouter(ApplicationDbContext context) : IApprovalRouter
{
    public async Task<IReadOnlyList<ApprovalRouteStep>> ResolveRouteAsync(
        Guid costCentreId,
        decimal amount,
        string currency,
        CancellationToken ct = default)
    {
        var policies = await context.ApprovalPolicies
            .AsNoTracking()
            .Where(p => p.IsActive
                        && p.Currency == currency
                        && p.MinimumAmount <= amount
                        && (p.CostCentreId == null || p.CostCentreId == costCentreId))
            .OrderBy(p => p.Sequence)
            .ToListAsync(ct);

        if (policies.Count == 0)
        {
            return [];
        }

        // A cost-centre-specific policy at a given sequence overrides the
        // organisation-wide default at the same sequence.
        var effective = policies
            .GroupBy(p => p.Sequence)
            .Select(g => g.OrderByDescending(p => p.CostCentreId.HasValue).First())
            .OrderBy(p => p.Sequence)
            .ToList();

        var departmentHeadId = await context.CostCentres
            .AsNoTracking()
            .Where(c => c.Id == costCentreId)
            .Select(c => c.DepartmentHeadUserId)
            .FirstOrDefaultAsync(ct);

        var route = new List<ApprovalRouteStep>(effective.Count);
        var sequence = 1;

        foreach (var policy in effective)
        {
            var assigned = policy.RequiredRole == Roles.DepartmentHead
                ? departmentHeadId
                : null;

            route.Add(new ApprovalRouteStep(sequence++, policy.RequiredRole, assigned));
        }

        return route;
    }
}
