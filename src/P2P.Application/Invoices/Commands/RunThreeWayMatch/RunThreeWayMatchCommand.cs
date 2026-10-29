using P2P.Application.Abstractions.Messaging;

namespace P2P.Application.Invoices.Commands.RunThreeWayMatch;

public sealed record RunThreeWayMatchCommand(Guid InvoiceId) : ICommand;
