using Application.Common;
using Application.Features.Permissions.Commands;
using Application.Features.Permissions.Queries;
using Infrastructure.Authentication;
using MediatR;
using Shared.Results;

namespace Api.Endpoints.V1;

public class PermissionEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var permissions = app.MapGroup("/permissions")
            .WithTags("Permissions")
            .RequireAuthorization()
            .RequireRateLimiting("default");

        permissions.MapGet("/",
                           async (ISender sender, CancellationToken cancellationToken) =>
                               Results.Ok(new ApiEnvelope<object>(
                                              await sender.Send(new ListPermissionsQuery(), cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.PermissionsRead));

        permissions.MapPost("/",
                            async (CreatePermissionCommand command, ISender sender,
                                    CancellationToken cancellationToken) =>
                                Results.Ok(new ApiEnvelope<object>(await sender.Send(command, cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.PermissionsManage));

        permissions.MapPut("/{permissionId:guid}",
                           async (Guid permissionId, UpdatePermissionRequest request, ISender sender,
                                   CancellationToken cancellationToken) =>
                               Results.Ok(new ApiEnvelope<object>(
                                              await sender.Send(
                                                  new UpdatePermissionCommand(
                                                      permissionId,
                                                      request.Name,
                                                      request.Resource,
                                                      request.Action,
                                                      request.Description,
                                                      request.IsActive),
                                                  cancellationToken))))
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.PermissionsManage));

        permissions.MapDelete("/{permissionId:guid}",
                              async (Guid permissionId, ISender sender, CancellationToken cancellationToken) =>
                              {
                                  await sender.Send(new DeletePermissionCommand(permissionId), cancellationToken);
                                  return Results.NoContent();
                              })
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.PermissionsManage));
    }
}

public sealed record UpdatePermissionRequest(
    string Name,
    string Resource,
    string Action,
    string? Description,
    bool IsActive);