using P2P.Domain.Common;

namespace P2P.Domain.Invoices;

/// <summary>
/// The permitted deviation between purchase order, goods receipt and
/// invoice before an exception is raised. Sourced from configuration;
/// see chapter 9 for how it is persisted per vendor.
/// </summary>
public sealed record MatchTolerance(
    decimal QuantityPercent,
    decimal PricePercent,
    decimal AbsoluteFloorAmount,
    string Currency)
{
    public static MatchTolerance Default(string currency) =>
        new(QuantityPercent: 0m,
            PricePercent: 2m,
            AbsoluteFloorAmount: 1000m,
            Currency: currency);

    public Money AbsoluteFloor => new(AbsoluteFloorAmount, Currency);

    public bool IsWithinAbsoluteFloor(Money difference) =>
        difference <= AbsoluteFloor;
}
