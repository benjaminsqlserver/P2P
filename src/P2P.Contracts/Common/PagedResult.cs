namespace P2P.Contracts.Common;

/// <summary>
/// A page of results plus the total row count, which the Radzen DataGrid
/// needs in order to render its pager.
/// </summary>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int TotalCount { get; init; }

    public static PagedResult<T> Empty() =>
        new() { Items = [], TotalCount = 0 };
}
