using FluentValidation;
using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.PurchaseOrders;
using P2P.Domain.Common;
using P2P.Domain.PurchaseOrders;
using P2P.Domain.Requisitions;
using P2P.Domain.Vendors;

namespace P2P.Application.PurchaseOrders.Commands.CreatePurchaseOrderFromRequisition;

public sealed record CreatePurchaseOrderFromRequisitionCommand(
    CreatePurchaseOrderFromRequisitionRequest Request) : ICommand<Guid>;

internal sealed class CreatePurchaseOrderFromRequisitionCommandValidator
    : AbstractValidator<CreatePurchaseOrderFromRequisitionCommand>
{
    public CreatePurchaseOrderFromRequisitionCommandValidator()
    {
        RuleFor(c => c.Request.RequisitionId).NotEmpty();
        RuleFor(c => c.Request.VendorId).NotEmpty();
        RuleFor(c => c.Request.TaxRatePercent).InclusiveBetween(0m, 100m);

        RuleFor(c => c.Request.ShipToAddress).NotNull();
        RuleFor(c => c.Request.ShipToAddress.Line1).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Request.ShipToAddress.City).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Request.ShipToAddress.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Request.ShipToAddress.CountryCode).NotEmpty().Length(2);
    }
}

internal sealed class CreatePurchaseOrderFromRequisitionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock,
    INumberSequence numbers)
    : ICommandHandler<CreatePurchaseOrderFromRequisitionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreatePurchaseOrderFromRequisitionCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var requisition = await context.Requisitions
            .Include(r => r.Lines)
            .Include(r => r.ApprovalSteps)
            .SingleOrDefaultAsync(r => r.Id == request.RequisitionId, ct);

        if (requisition is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(nameof(Requisition), request.RequisitionId));
        }

        var vendor = await context.Vendors
            .SingleOrDefaultAsync(v => v.Id == request.VendorId, ct);

        if (vendor is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(nameof(Vendor), request.VendorId));
        }

        var shipTo = new Address(
            request.ShipToAddress.Line1,
            request.ShipToAddress.Line2,
            request.ShipToAddress.City,
            request.ShipToAddress.StateOrProvince,
            request.ShipToAddress.PostalCode,
            request.ShipToAddress.CountryCode.ToUpperInvariant());

        var number = await numbers.NextAsync("PO", ct);

        var orderResult = PurchaseOrder.CreateFromRequisition(
            number,
            requisition,
            vendor,
            currentUser.UserId,
            clock.Today,
            request.ExpectedDeliveryDate,
            shipTo,
            request.TaxRatePercent);

        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        // Converting in the same transaction is what stops one approved
        // requisition being raised as two purchase orders.
        var converted = requisition.MarkConverted();

        if (converted.IsFailure)
        {
            return Result.Failure<Guid>(converted.Error);
        }

        context.PurchaseOrders.Add(orderResult.Value);

        return orderResult.Value.Id;
    }
}
