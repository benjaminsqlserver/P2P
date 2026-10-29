using P2P.Application.Abstractions.Messaging;

namespace P2P.Application.Requisitions.Commands.ApproveRequisition;

public sealed record ApproveRequisitionCommand(Guid RequisitionId, string? Comment)
    : ICommand;
