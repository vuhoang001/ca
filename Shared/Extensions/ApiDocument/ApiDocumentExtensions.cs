using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Auth.Shared.Extensions.ApiDocument;

/// <summary>
/// Extension methods for API documentation middleware configuration
/// Sets up Swagger UI and JSON endpoint with proxy-aware URL handling
/// </summary>
public static class ApiDocumentAppExtensions
{
    public static void UseApiDocument(this WebApplication app)
    {
        // Only enable Swagger in Development environment
        if (!app.Environment.IsDevelopment())
            return;

        var opts = app.Services.GetRequiredService<IOptions<ApiDocumentOptions>>().Value;
        var logger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ApiDocument");

        logger.LogInformation("Swagger enabled for service: {ServiceTitle}", opts.ServiceTitle);

        // Configure Swagger JSON endpoint
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";

            // Inject proper server URL considering X-Forwarded headers from YARP proxy
            c.PreSerializeFilters.Add((swagger, req) =>
            {
                var proto  = req.Headers["X-Forwarded-Proto"].FirstOrDefault()  ?? req.Scheme;
                var host   = req.Headers["X-Forwarded-Host"].FirstOrDefault()   ?? req.Host.Value;
                var prefix = req.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? opts.FallbackPrefix;

                swagger.Servers = [new OpenApiServer() { Url = $"{proto}://{host}{prefix}" }];
            });
        });

        // Configure Swagger UI
        app.UseSwaggerUI(ui =>
        {
            // UI is served at /swagger root
            ui.RoutePrefix = "swagger";

            // Use RELATIVE paths so URLs work correctly behind YARP proxy
            // Each version gets its own tab in Swagger UI dropdown
            foreach (var v in opts.Versions)
            {
                ui.SwaggerEndpoint($"{v}/swagger.json", v.ToUpperInvariant());
            }
        });

        logger.LogInformation("Swagger UI configured with {VersionCount} versions: {Versions}",
                              opts.Versions.Length,
                              string.Join(", ", opts.Versions));
    }
}