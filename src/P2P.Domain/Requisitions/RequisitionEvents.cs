using P2P.Domain.Common;

namespace P2P.Domain.Requisitions;

public sealed record RequisitionCreated(
    Guid RequisitionId, string Number, Guid RequesterId) : DomainEvent;

public sealed record RequisitionSubmitted(
    Guid RequisitionId, string Number, Guid RequesterId,
    decimal Total, string Currency) : DomainEvent;

public sealed record RequisitionStepApproved(
    Guid RequisitionId, string Number, int Sequence, Guid ApproverId) : DomainEvent;

public sealed record RequisitionApproved(
    Guid RequisitionId, string Number, Guid RequesterId,
    decimal Total, string Currency) : DomainEvent;

public sealed record RequisitionRejected(
    Guid RequisitionId, string Number, Guid RequesterId, string Reason) : DomainEvent;

public sealed record RequisitionCancelled(
    Guid RequisitionId, string Number, Guid ActorId) : DomainEvent;
