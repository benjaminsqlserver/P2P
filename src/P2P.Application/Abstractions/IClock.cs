namespace P2P.Application.Abstractions;

/// <summary>
/// Time as a dependency. Handlers must never call DateTimeOffset.UtcNow
/// directly; a test that cannot control the clock cannot test an ageing
/// report or a payment due date.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}
