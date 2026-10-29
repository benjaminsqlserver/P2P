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
using P2P.Domain.Common;
using P2P.Web.Endpoints;

namespace P2P.Web.Services;

/// <summary>
/// The in-process implementation of <see cref="IVendorApi"/>. Forwards each
/// call to the CQRS dispatcher without an HTTP hop, which is what makes
/// server-side prerendering fast.
/// </summary>
internal sealed class ServerVendorApi(IDispatcher dispatcher) : IVendorApi
{
    public async Task<PagedResult<VendorListItemDto>> GetListAsync(
        VendorGridQuery query, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetVendorListQuery(query), ct);

        return result.IsSuccess
            ? result.Value
            : PagedResult<VendorListItemDto>.Empty();
    }

    public async Task<VendorDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetVendorByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<IReadOnlyList<VendorLookupDto>> SearchLookupAsync(
        string? term, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new SearchVendorLookupQuery(term), ct);
        return result.IsSuccess ? result.Value : [];
    }

    public async Task<OperationResult<Guid>> CreateAsync(
        CreateVendorRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new CreateVendorCommand(request), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> UpdateAsync(
        UpdateVendorRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new UpdateVendorCommand(request), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> ActivateAsync(
        Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new ActivateVendorCommand(id), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> PlaceOnHoldAsync(
        Guid id, string reason, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(
            new PlaceVendorOnHoldCommand(id, reason), ct);

        return result.ToOperationResult();
    }

    public async Task<OperationResult> AddBankAccountAsync(
        AddBankAccountRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new AddBankAccountCommand(request), ct);
        return ((Result)result).ToOperationResult();
    }
}
