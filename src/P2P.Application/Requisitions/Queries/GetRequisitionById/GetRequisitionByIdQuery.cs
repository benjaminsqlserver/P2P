using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Requisitions;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Queries.GetRequisitionById;

public sealed record GetRequisitionByIdQuery(Guid RequisitionId)
    : IQuery<RequisitionDetailDto>;

internal sealed class GetRequisitionByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetRequisitionByIdQuery, RequisitionDetailDto>
{
    public async Task<Result<RequisitionDetailDto>> Handle(
        GetRequisitionByIdQuery request, CancellationToken ct)
    {
        var requisition = await context.Requisitions
            .AsNoTracking()
            .Include(r => r.Lines)
            .Include(r => r.ApprovalSteps)
            .SingleOrDefaultAsync(r => r.Id == request.RequisitionId, ct);

        if (requisition is null)
        {
            return Result.Failure<RequisitionDetailDto>(
                Error.NotFound(nameof(Requisition), request.RequisitionId));
        }

        var costCentreCode = await context.CostCentres
            .AsNoTracking()
            .Where(c => c.Id == requisition.CostCentreId)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(ct) ?? "—";

        return new RequisitionDetailDto
        {
            Id = requisition.Id,
            Number = requisition.Number,
            Title = requisition.Title,
            Justification = requisition.Justification,
            RequesterId = requisition.RequesterId,
            RequesterName = "—",
            CostCentreId = requisition.CostCentreId,
            CostCentreCode = costCentreCode,
            Status = (RequisitionStatusDto)(int)requisition.Status,
            NeededBy = requisition.NeededBy,
            Currency = requisition.Currency,
            Total = requisition.Total.Amount,
            RejectionReason = requisition.RejectionReason,
            Lines = requisition.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new RequisitionLineDto
                {
                    Id = l.Id,
                    LineNumber = l.LineNumber,
                    ItemCode = l.ItemCode,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitOfMeasure = l.UnitOfMeasure,
                    UnitPrice = l.UnitPrice.Amount,
                    LineTotal = l.LineTotal.Amount
                })
                .ToList(),
            ApprovalSteps = requisition.ApprovalSteps
                .OrderBy(s => s.Sequence)
                .Select(s => new ApprovalStepDto
                {
                    Id = s.Id,
                    Sequence = s.Sequence,
                    RequiredRole = s.RequiredRole,
                    Status = s.Status.ToString(),
                    ActionedAtUtc = s.ActionedAtUtc,
                    Comment = s.Comment
                })
                .ToList()
        };
    }
}
