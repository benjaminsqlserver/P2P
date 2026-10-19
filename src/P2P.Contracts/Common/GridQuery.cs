namespace P2P.Contracts.Common;

/// <summary>
/// The server-side paging, sorting and searching parameters produced from a
/// Radzen <c>LoadDataArgs</c>. Deliberately strongly typed rather than an
/// OData filter string: the query surface is then explicit, auditable, and
/// impossible to abuse.
/// </summary>
public record GridQuery
{
    public int Skip { get; init; }

    public int Top { get; init; } = 25;

    /// <summary>Property name to sort by, as exposed on the DTO.</summary>
    public string? OrderBy { get; init; }

    public bool Descending { get; init; }

    /// <summary>Free-text search across the fields the query decides to cover.</summary>
    public string? Search { get; init; }
}
