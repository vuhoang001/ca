using Api.Extensions;

namespace Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApiV1Endpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup(ApiVersions.V1).WithOpenApi();

        IEndpointModule[] modules =
        [
            new V1.AuthEndpoints(),
            new V1.UserEndpoints(),
            new V1.RoleEndpoints(),
            new V1.PermissionEndpoints(),
            new V1.HealthEndpoints()
        ];


        foreach (var endpointModule in modules)
        {
            endpointModule.MapEndpoints(api);
        }

        return app;
    }
}