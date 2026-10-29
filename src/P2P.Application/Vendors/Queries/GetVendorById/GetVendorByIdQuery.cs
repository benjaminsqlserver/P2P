using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Vendors;

namespace P2P.Application.Vendors.Queries.GetVendorById;

public sealed record GetVendorByIdQuery(Guid VendorId)
    : IQuery<VendorDetailDto>;
