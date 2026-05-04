
using Application.Common;
using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using Infrastructure.Authentication;
using MediatR;
using Shared.Results;

namespace Api.Endpoints.V1;

public sealed class UserEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization()
            .RequireRateLimiting("default");

        users.MapPost("/{userId:guid}/roles",
                      async (Guid userId, AssignRolesRequest request, ISender sender,
                          CancellationToken cancellationToken) =>
                      {
                          await sender.Send(new AssignRolesToUserCommand(userId, request.RoleIds), cancellationToken);
                          return Results.NoContent();
                      })
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.UsersManage));

        users.MapGet("/{userId:guid}/permissions/{permissionCode}",
                     async (Guid userId, string permissionCode, ISender sender, CancellationToken cancellationToken) =>
                         Results.Ok(new ApiEnvelope<object>(
                                        await sender.Send(new CheckUserPermissionQuery(userId, permissionCode),
                                                          cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.UsersRead));
    }
}

public sealed record AssignRolesRequest(IReadOnlyCollection<Guid> RoleIds);