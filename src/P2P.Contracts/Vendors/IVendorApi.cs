using P2P.Contracts.Common;

namespace P2P.Contracts.Vendors;

/// <summary>
/// The vendor surface as the UI sees it. Implemented twice: in-process on
/// the server, over HTTP in the browser. Blazor components depend on this
/// interface and never on anything below it.
/// </summary>
public interface IVendorApi
{
    Task<PagedResult<VendorListItemDto>> GetListAsync(
        VendorGridQuery query, CancellationToken ct = default);

    Task<VendorDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<VendorLookupDto>> SearchLookupAsync(
        string? term, CancellationToken ct = default);

    Task<OperationResult<Guid>> CreateAsync(
        CreateVendorRequest request, CancellationToken ct = default);

    Task<OperationResult> UpdateAsync(
        UpdateVendorRequest request, CancellationToken ct = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken ct = default);

    Task<OperationResult> PlaceOnHoldAsync(
        Guid id, string reason, CancellationToken ct = default);

    Task<OperationResult> AddBankAccountAsync(
        AddBankAccountRequest request, CancellationToken ct = default);
}
