using P2P.Application.Abstractions.Messaging;

namespace P2P.Application.Vendors.Commands.ActivateVendor;

public sealed record ActivateVendorCommand(Guid VendorId) : ICommand;
