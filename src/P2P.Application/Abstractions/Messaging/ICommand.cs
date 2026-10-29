namespace P2P.Application.Abstractions.Messaging;

/// <summary>Marker for a request that changes state.</summary>
public interface ICommand<out TResponse>;

/// <summary>Marker for a request that changes state and returns nothing useful.</summary>
public interface ICommand : ICommand<Unit>;

/// <summary>Marker for a request that reads state and must not change it.</summary>
public interface IQuery<out TResponse>;

/// <summary>
/// The absence of a meaningful return value, for commands that only succeed
/// or fail. Preferable to <c>Task</c> because it keeps one generic signature
/// across the whole dispatcher.
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
