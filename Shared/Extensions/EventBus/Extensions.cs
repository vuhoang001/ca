using System.ComponentModel.DataAnnotations;
using Shared.Shared.Aspire;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Auth.Shared.Extensions.EventBus;

public static class Extensions
{
    public static void AddEventBus(
        this IHostApplicationBuilder builder,
        Type type,
        Action<IBusRegistrationConfigurator>? busConfigure = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? rabbitMqConfigure = null
    )
    {
        var connectionString = builder.Configuration.GetConnectionString(Components.Queue);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        builder.Services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.AddConsumers(type.Assembly);

            config.AddActivities(type.Assembly);


            config.AddRequestClient(type);
            busConfigure?.Invoke(config);

            config.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(connectionString));
                    configurator.ConfigureEndpoints(context);
                    configurator.UseMessageRetry(AddRetryConfiguration);
                    rabbitMqConfigure?.Invoke(context, configurator);
                }
            );

        });

        // builder
        //     .Services.AddOpenTelemetry()
        //     .WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
        //     .WithTracing(p => p.AddSource(DiagnosticHeaders.DefaultListenerName));
    }

    private static void AddRetryConfiguration(IRetryConfigurator retryConfigurator)
    {
        retryConfigurator
            .Exponential(
                3,
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMinutes(120),
                TimeSpan.FromMilliseconds(200)
            )
            .Ignore<ValidationException>();
    }
}
