using Serilog.Context;

namespace Threes.Registration.Api.Middleware;

// gives every request a correlation id. if the caller sent one we keep it,
// otherwise we mint one. the id is echoed back on the response and pushed onto
// the serilog log context so every log line for this request, across all
// layers, carries the same CorrelationId property. that is what lets you grep
// one registration attempt end to end.
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var existing) &&
            !string.IsNullOrWhiteSpace(existing))
        {
            return existing.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}
