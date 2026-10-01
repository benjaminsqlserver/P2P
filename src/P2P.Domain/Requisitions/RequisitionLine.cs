using P2P.Domain.Common;

namespace P2P.Domain.Requisitions;

public sealed class RequisitionLine : Entity
{
    private RequisitionLine(
        Guid id,
        Guid requisitionId,
        int lineNumber,
        string? itemCode,
        string description,
        decimal quantity,
        string unitOfMeasure,
        Money unitPrice,
        Guid? suggestedVendorId)
        : base(id)
    {
        RequisitionId = requisitionId;
        LineNumber = lineNumber;
        ItemCode = itemCode;
        Description = description;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        SuggestedVendorId = suggestedVendorId;
    }

    private RequisitionLine() { }

    public Guid RequisitionId { get; private set; }

    public int LineNumber { get; private set; }

    public string? ItemCode { get; private set; }

    public string Description { get; private set; } = null!;

    public decimal Quantity { get; private set; }

    public string UnitOfMeasure { get; private set; } = null!;

    public Money UnitPrice { get; private set; }

    public Guid? SuggestedVendorId { get; private set; }

    public Money LineTotal => UnitPrice * Quantity;

    internal static Result<RequisitionLine> Create(
        Guid requisitionId,
        int lineNumber,
        string? itemCode,
        string description,
        decimal quantity,
        string unitOfMeasure,
        Money unitPrice,
        Guid? suggestedVendorId)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<RequisitionLine>(
                RequisitionErrors.LineDescriptionRequired);
        }

        if (quantity <= 0)
        {
            return Result.Failure<RequisitionLine>(
                RequisitionErrors.LineQuantityMustBePositive);
        }

        if (unitPrice.IsNegative)
        {
            return Result.Failure<RequisitionLine>(
                RequisitionErrors.LinePriceMustNotBeNegative);
        }

        return new RequisitionLine(
            Guid.CreateVersion7(), requisitionId, lineNumber,
            string.IsNullOrWhiteSpace(itemCode) ? null : itemCode.Trim(),
            description.Trim(), quantity, unitOfMeasure, unitPrice,
            suggestedVendorId);
    }

    internal void Renumber(int lineNumber) => LineNumber = lineNumber;
}
