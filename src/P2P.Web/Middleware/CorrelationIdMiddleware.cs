using Serilog.Context;

namespace P2P.Web.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(
            HeaderName, out var supplied) && !string.IsNullOrWhiteSpace(supplied)
                ? supplied.ToString()
                : Guid.CreateVersion7().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
