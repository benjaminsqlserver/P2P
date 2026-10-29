using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Requisitions;

namespace P2P.Application.Requisitions.Commands.CreateRequisition;

public sealed record CreateRequisitionCommand(CreateRequisitionRequest Request)
    : ICommand<Guid>;
