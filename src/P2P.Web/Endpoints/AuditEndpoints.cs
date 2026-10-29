using P2P.Application.Abstractions.Messaging;
using P2P.Application.Audit.Queries.GetAuditTrail;
using P2P.Contracts.Common;

namespace P2P.Web.Endpoints;

public sealed class AuditEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit")
            .WithTags("Audit")
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.Auditor, Roles.ProcurementAdministrator));

        group.MapPost("/search",
            async (GridQuery query, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetAuditTrailQuery(query), ct);
                return result.ToHttpResult();
            })
            .WithName("GetAuditTrail");
    }
}
