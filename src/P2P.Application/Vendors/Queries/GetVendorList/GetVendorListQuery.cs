using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Common;
using P2P.Contracts.Vendors;

namespace P2P.Application.Vendors.Queries.GetVendorList;

public sealed record GetVendorListQuery(VendorGridQuery Query)
    : IQuery<PagedResult<VendorListItemDto>>;
