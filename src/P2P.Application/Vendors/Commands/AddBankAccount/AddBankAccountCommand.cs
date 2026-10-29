using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Vendors;

namespace P2P.Application.Vendors.Commands.AddBankAccount;

public sealed record AddBankAccountCommand(AddBankAccountRequest Request)
    : ICommand<Guid>;
