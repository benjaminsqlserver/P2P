namespace P2P.Application.Abstractions;

/// <summary>
/// Allocates gap-free, human-readable document numbers such as
/// REQ-2026-000148. Implemented against a SQL Server table in chapter 9.
/// </summary>
public interface INumberSequence
{
    Task<string> NextAsync(string documentType, CancellationToken ct = default);
}
