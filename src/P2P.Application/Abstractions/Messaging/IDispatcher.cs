using P2P.Domain.Common;

namespace P2P.Application.Abstractions.Messaging;

public interface IDispatcher
{
    Task<Result<TResponse>> Send<TResponse>(
        ICommand<TResponse> command, CancellationToken ct = default);

    Task<Result<TResponse>> Query<TResponse>(
        IQuery<TResponse> query, CancellationToken ct = default);
}
