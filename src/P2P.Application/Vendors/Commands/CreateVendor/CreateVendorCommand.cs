using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Vendors;

namespace P2P.Application.Vendors.Commands.CreateVendor;

public sealed record CreateVendorCommand(CreateVendorRequest Request)
    : ICommand<Guid>;
