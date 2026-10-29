using P2P.Domain.Common;

namespace P2P.Application.Abstractions.Messaging;

public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();

/// <summary>
/// A cross-cutting concern wrapped around every handler: validation,
/// logging, transactions, caching, authorisation.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct);
}
