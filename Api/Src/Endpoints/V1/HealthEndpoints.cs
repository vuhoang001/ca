using Shared.Results;

namespace Api.Endpoints.V1;

public class HealthEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new ApiEnvelope<object>(new { status = "ok" })))
            .AllowAnonymous();
    }
}