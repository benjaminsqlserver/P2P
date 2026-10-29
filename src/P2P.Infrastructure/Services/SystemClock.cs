using P2P.Application.Abstractions;

namespace P2P.Infrastructure.Services;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
