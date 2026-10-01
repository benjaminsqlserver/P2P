namespace P2P.Domain.Common;

/// <summary>
/// A stable, machine-readable description of a business rule violation.
/// Codes are part of the application's public contract: never change one
/// without treating it as a breaking change.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"{entity} with identifier '{id}' was not found.");

    public static Error Conflict(string code, string description) =>
        new(code, description);

    public static Error Validation(string code, string description) =>
        new(code, description);

    public override string ToString() => Code;
}
