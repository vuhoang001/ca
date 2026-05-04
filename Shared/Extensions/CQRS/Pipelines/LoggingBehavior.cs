using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Shared.Extensions.CQRS.Pipelines;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string behavior = "LoggingBehavior";

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[{Behavior}] Handle request={Request} and response={Response}",
                behavior,
                typeof(TRequest).Name,
                typeof(TResponse).Name
            );

            var props = new List<PropertyInfo>(request.GetType().GetProperties());
            foreach (var prop in props)
            {
                var propValue = prop.GetValue(request, null);
                logger.LogInformation(
                    "[{Behavior}] Property {Property} : {@Value}",
                    behavior,
                    prop.Name,
                    propValue
                );
            }
        }

        var start = Stopwatch.GetTimestamp();
        var response = await next();
        var timeTaken = Stopwatch.GetElapsedTime(start);

        const int threshold = 3;

        if (timeTaken.Seconds >= threshold)
        {
            logger.LogWarning(
                "[{Behavior}] The request {Request} took {TimeTaken} seconds.",
                behavior,
                typeof(TRequest).Name,
                timeTaken.Seconds
            );
        }
        else
        {
            logger.LogInformation(
                "[{Behavior}] The request handled {RequestName} with {Response} in {ElapsedMilliseconds} ms",
                behavior,
                typeof(TRequest).Name,
                response,
                timeTaken.TotalMilliseconds
            );
        }

        return response;
    }
}