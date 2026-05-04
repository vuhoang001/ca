using Shared.Extensions.CQRS.Pipelines;
using Shared.Extensions.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class Extensions
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddMediatR(typeof(IBasketMarker).Assembly)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));


        #region Add exception handlers

        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        #endregion
    }
}