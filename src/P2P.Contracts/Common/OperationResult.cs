namespace P2P.Contracts.Common;

/// <summary>
/// The wire form of a domain <c>Result</c>. Carries the stable error code so
/// the UI can localise or branch, and a human-readable message for display.
/// </summary>
public sealed record OperationResult
{
    public required bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static OperationResult Success() => new() { Succeeded = true };

    public static OperationResult Failure(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}

public sealed record OperationResult<T>
{
    public required bool Succeeded { get; init; }

    public T? Value { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public static OperationResult<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static OperationResult<T> Failure(string code, string message) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
}
