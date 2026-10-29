using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Vendors;

namespace P2P.Application.Vendors.Commands.UpdateVendor;

public sealed record UpdateVendorCommand(UpdateVendorRequest Request) : ICommand;
