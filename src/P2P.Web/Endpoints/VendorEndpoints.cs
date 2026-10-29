using P2P.Application.Abstractions.Messaging;
using P2P.Application.Vendors.Commands.ActivateVendor;
using P2P.Application.Vendors.Commands.AddBankAccount;
using P2P.Application.Vendors.Commands.CreateVendor;
using P2P.Application.Vendors.Commands.PlaceVendorOnHold;
using P2P.Application.Vendors.Commands.UpdateVendor;
using P2P.Application.Vendors.Queries.GetVendorById;
using P2P.Application.Vendors.Queries.GetVendorList;
using P2P.Application.Vendors.Queries.SearchVendorLookup;
using P2P.Contracts.Common;
using P2P.Contracts.Vendors;

namespace P2P.Web.Endpoints;

public sealed class VendorEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vendors")
            .WithTags("Vendors")
            .RequireAuthorization();

        group.MapPost("/search",
            async (VendorGridQuery query, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetVendorListQuery(query), ct);
                return result.ToHttpResult();
            })
            .WithName("SearchVendors");

        group.MapGet("/{id:guid}",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetVendorByIdQuery(id), ct);
                return result.ToHttpResult();
            })
            .WithName("GetVendorById");

        group.MapGet("/lookup",
            async (string? term, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(
                    new SearchVendorLookupQuery(term), ct);
                return result.ToHttpResult();
            })
            .WithName("SearchVendorLookup");

        group.MapPost("/",
            async (CreateVendorRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreateVendorCommand(request), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.VendorManager,
                                   Roles.ProcurementAdministrator))
            .WithName("CreateVendor");

        group.MapPost("/{id:guid}/activate",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new ActivateVendorCommand(id), ct);
                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.VendorManager,
                                   Roles.ProcurementAdministrator))
            .WithName("ActivateVendor");

        group.MapPut("/{id:guid}",
            async (Guid id,
                   UpdateVendorRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new UpdateVendorCommand(request with { Id = id }), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.VendorManager,
                                   Roles.ProcurementAdministrator))
            .WithName("UpdateVendor");

        group.MapPost("/{id:guid}/hold",
            async (Guid id,
                   ReasonRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new PlaceVendorOnHoldCommand(id, request.Reason), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.VendorManager,
                                   Roles.ProcurementAdministrator))
            .WithName("PlaceVendorOnHold");

        group.MapPost("/{id:guid}/bank-accounts",
            async (Guid id,
                   AddBankAccountRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new AddBankAccountCommand(request with { VendorId = id }), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.VendorManager,
                                   Roles.ProcurementAdministrator))
            .WithName("AddVendorBankAccount");
    }
}
