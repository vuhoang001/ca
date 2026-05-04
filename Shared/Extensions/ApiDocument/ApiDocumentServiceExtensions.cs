using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Auth.Shared.Extensions.ApiDocument;

/// <summary>
/// Extension methods for adding API documentation service configuration
/// Handles Swagger generation with version-based filtering
/// </summary>
public static class ApiDocumentServiceExtensions
{
    public static void AddApiDocument(this IServiceCollection services,
        string title,
        Action<ApiDocumentBuilder>? configure = null
    )
    {
        var builder = new ApiDocumentBuilder(title);
        configure?.Invoke(builder);
        var opts = builder.Build();

        services.AddSingleton(Options.Create(opts));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // Create separate Swagger document for each version
            foreach (var ver in opts.Versions)
            {
                c.SwaggerDoc(ver, new OpenApiInfo()
                {
                    Title       = opts.ServiceTitle,
                    Version     = ver.ToUpperInvariant(),
                    Description = opts.Description ?? $"{opts.ServiceTitle} – {ver.ToUpperInvariant()}",
                });
            }

            // Filter endpoints to include only those matching the current document version
            c.DocInclusionPredicate((docName, api) =>
            {
                // First, check if endpoint has explicit GroupName (from API versioning)
                if (!string.IsNullOrEmpty(api.GroupName))
                {
                    return api.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
                }

                // Fall back to path-based matching: /v1/endpoint, /v2/endpoint, etc.
                var path = api.RelativePath ?? string.Empty;
                return path.StartsWith(docName + "/", StringComparison.OrdinalIgnoreCase)
                    || path.Equals(docName, StringComparison.OrdinalIgnoreCase);
            });

            // Add JWT Bearer authentication if configured
            if (opts.UseJwtAuth)
            {
                c.AddJwtAuth();
            }
        });
    }
}