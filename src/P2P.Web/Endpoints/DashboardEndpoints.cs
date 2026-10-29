using P2P.Application.Abstractions.Messaging;
using P2P.Application.Dashboard.Queries.GetDashboardSummary;
using P2P.Application.Dashboard.Queries.GetInvoiceAgeing;
using P2P.Application.Dashboard.Queries.GetMonthlySpend;

namespace P2P.Web.Endpoints;

public sealed class DashboardEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        group.MapGet("/summary",
            async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetDashboardSummaryQuery(), ct);
                return result.ToHttpResult();
            })
            .WithName("GetDashboardSummary");

        group.MapGet("/monthly-spend",
            async (int months, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetMonthlySpendQuery(months), ct);
                return result.ToHttpResult();
            })
            .WithName("GetMonthlySpend");

        group.MapGet("/ageing",
            async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetInvoiceAgeingQuery(), ct);
                return result.ToHttpResult();
            })
            .WithName("GetInvoiceAgeing");
    }
}
