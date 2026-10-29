using P2P.Application.Abstractions.Messaging;
using P2P.Application.Organisation.Queries.GetCostCentreLookup;

namespace P2P.Web.Endpoints;

public sealed class OrganisationEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organisation")
            .WithTags("Organisation")
            .RequireAuthorization();

        group.MapGet("/cost-centres/lookup",
            async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetCostCentreLookupQuery(), ct);
                return result.ToHttpResult();
            })
            .WithName("GetCostCentreLookup");
    }
}
