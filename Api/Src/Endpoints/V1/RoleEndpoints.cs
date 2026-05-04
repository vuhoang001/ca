using Application.Common;
using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using Infrastructure.Authentication;
using MediatR;
using Shared.Results;

namespace Api.Endpoints.V1;

public class RoleEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var roles = app.MapGroup("/roles")
            .WithTags("Roles")
            .RequireAuthorization()
            .RequireRateLimiting("default");

        roles.MapGet("/",
                     async (Guid? tenantId, ISender sender, CancellationToken cancellationToken) =>
                         Results.Ok(new ApiEnvelope<object>(
                                        await sender.Send(new ListRolesQuery(tenantId), cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.RolesRead));

        roles.MapPost("/",
                      async (CreateRoleCommand command, ISender sender, CancellationToken cancellationToken) =>
                          Results.Ok(new ApiEnvelope<object>(await sender.Send(command, cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.RolesManage));

        roles.MapPut("/{roleId:guid}",
                     async (Guid roleId, UpdateRoleRequest request, ISender sender,
                             CancellationToken cancellationToken) =>
                         Results.Ok(new ApiEnvelope<object>(
                                        await sender.Send(
                                            new UpdateRoleCommand(roleId, request.Name, request.Description,
                                                                  request.IsActive),
                                            cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.RolesManage));

        roles.MapDelete("/{roleId:guid}",
                        async (Guid roleId, ISender sender, CancellationToken cancellationToken) =>
                        {
                            await sender.Send(new DeleteRoleCommand(roleId), cancellationToken);
                            return Results.NoContent();
                        })
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.RolesManage));

        roles.MapPost("/{roleId:guid}/permissions",
                      async (Guid roleId, AssignPermissionsRequest request, ISender sender,
                          CancellationToken cancellationToken) =>
                      {
                          await sender.Send(
                              new AssignPermissionsToRoleCommand(roleId, request.PermissionIds),
                              cancellationToken);

                          return Results.NoContent();
                      })
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.RolesManage));
    }
}

public sealed record UpdateRoleRequest(string Name, string? Description, bool IsActive);

public sealed record AssignPermissionsRequest(IReadOnlyCollection<Guid> PermissionIds);