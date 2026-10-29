using P2P.Application.Abstractions.Messaging;

namespace P2P.Application.Vendors.Commands.PlaceVendorOnHold;

public sealed record PlaceVendorOnHoldCommand(Guid VendorId, string Reason) : ICommand;
