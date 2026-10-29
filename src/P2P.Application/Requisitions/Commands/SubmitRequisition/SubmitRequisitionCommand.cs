using P2P.Application.Abstractions.Messaging;

namespace P2P.Application.Requisitions.Commands.SubmitRequisition;

public sealed record SubmitRequisitionCommand(Guid RequisitionId) : ICommand;
