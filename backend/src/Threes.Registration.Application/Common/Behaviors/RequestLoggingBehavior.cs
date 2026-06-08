using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Threes.Registration.Application.Common.Behaviors;

// logs the start, finish and duration of every request that flows through
// mediatr. it logs the request type name and timing only, never the request
// body, so personal data (names, email, mobile) stays out of the logs. the
// correlation id is attached upstream by the api middleware and rides along on
// the logger scope, so these lines tie back to a single http request.
public sealed class RequestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<RequestLoggingBehavior<TRequest, TResponse>> _logger;

    public RequestLoggingBehavior(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("handling {RequestName}", requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();
            _logger.LogInformation(
                "handled {RequestName} in {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // log the type of failure and how long we spent, but let the global
            // handler decide the http status. still no request body here.
            _logger.LogWarning(
                "{RequestName} failed after {ElapsedMilliseconds} ms with {ExceptionType}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                ex.GetType().Name);
            throw;
        }
    }
}
