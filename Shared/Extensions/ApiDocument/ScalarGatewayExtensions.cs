using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;

namespace Auth.Shared.Extensions.ApiDocument;

public static class ScalarGatewayExtension
{
    // Overload A: đọc tự động từ YARP config
    public static IEndpointRouteBuilder MapScalarGateway(
        this IEndpointRouteBuilder app,
        IConfiguration config,
        string[]? versions = null,
        string scalarPath = "/scalar",
        Action<ScalarOptions>? scalarSetup = null)
    {
        var services = ExtractPrefixesFromYarp(config)
            .Select(p => (Prefix: p, Name: PrefixToDisplayName(p)))
            .ToArray();
#pragma warning disable CS0618 // Type or member is obsolete
        return MapScalarGatewayCore(app, scalarPath,
                                    versions ?? ["v1"], scalarSetup, services);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("Obsolete")]
    public static IEndpointRouteBuilder MapScalarGateway(
        this IEndpointRouteBuilder app,
        string[]? versions = null,
        string scalarPath = "/scalar",
        Action<ScalarOptions>? scalarSetup = null,
        params (string Prefix, string Name)[] services)
        => MapScalarGatewayCore(app, scalarPath,
                                versions ?? ["v1"], scalarSetup, services);

    // Core (private)
    [Obsolete("Obsolete")]
    private static IEndpointRouteBuilder MapScalarGatewayCore(
        IEndpointRouteBuilder app,
        string scalarPath,
        string[] versions,
        Action<ScalarOptions>? scalarSetup,
        (string Prefix, string Name)[] services)
    {
        app.MapScalarApiReference(scalarPath, (options, httpContext) =>
        {
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            foreach (var (prefix, name) in services)
            foreach (var version in versions)
            {
                options.AddDocument(
                    $"{prefix.Trim('/')}-{version}",
                    $"{name} - {version.ToUpperInvariant()}",
                    $"{baseUrl}{prefix}/swagger/{version}/swagger.json"
                );
            }

            // Tắt proxy.scalar.com — tránh rò rỉ Authorization headers
            options.WithProxyUrl(string.Empty);
            scalarSetup?.Invoke(options);
        });
        return app;
    }

    private static IEnumerable<string> ExtractPrefixesFromYarp(IConfiguration config) =>
        config.GetSection("ReverseProxy:Routes").GetChildren()
            .Select(r => r.GetSection("Match:Path").Value ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p.Split("/{")[0].Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct();

    // "/basket" → "Basket API"
    private static string PrefixToDisplayName(string prefix)
    {
        var s = prefix.Trim('/');
        return string.IsNullOrEmpty(s)
            ? "API"
            : $"{char.ToUpperInvariant(s[0])}{s[1..]} API";
    }
}