namespace Auth.Shared.Extensions.ApiDocument;


/// <summary>
/// API Documentation Configuration Options
/// Unified single system for API versioning and Swagger setup
/// </summary>
public sealed record ApiDocumentOptions
{
    /// <summary>Service name for Swagger title</summary>
    public string ServiceTitle { get; init; } = "API";
    
    /// <summary>Service description</summary>
    public string? Description { get; init; }
    
    /// <summary>API versions to expose (e.g., "v1", "v2")</summary>
    /// <remarks>MUST match actual versioned endpoints mapped via MapEndpoints</remarks>
    public string[] Versions { get; init; } = ["v1"];

    /// <summary>Enable JWT Bearer authentication in Swagger</summary>
    public bool UseJwtAuth { get; init; }
    
    /// <summary>Path prefix when behind YARP proxy (e.g., "/basket", "/catalog")</summary>
    public string FallbackPrefix { get; init; } = string.Empty;
}

/// <summary>
/// Fluent builder for API documentation configuration
/// </summary>
/// <example>
/// var builder = new ApiDocumentBuilder("My Service")
///     .WithVersions("v1", "v2")
///     .WithPrefix("/myservice")
///     .WithDescription("My Service API")
///     .WithJwtAuth();
/// var options = builder.Build();
/// </example>
public sealed class ApiDocumentBuilder(string title)
{
    private string?  _description;
    private string   _prefix   = string.Empty;
    private string[] _versions = ["v1"];
    private bool     _useJwtAuth;

    public ApiDocumentBuilder WithVersions(params string[] versions)
    {
        if (versions.Length == 0)
            throw new ArgumentException("At least one version is required");
            
        _versions = versions.Select(v => v.ToLowerInvariant()).ToArray();
        return this;
    }

    public ApiDocumentBuilder WithPrefix(string prefix)
    {
        _prefix = prefix;
        return this;
    }

    public ApiDocumentBuilder WithDescription(string desc)
    {
        _description = desc;
        return this;
    }

    public ApiDocumentBuilder WithJwtAuth()
    {
        _useJwtAuth = true;
        return this;
    }

    internal ApiDocumentOptions Build() => new()
    {
        ServiceTitle   = title,
        Description    = _description,
        Versions       = _versions,
        FallbackPrefix = _prefix,
        UseJwtAuth     = _useJwtAuth
    };
}