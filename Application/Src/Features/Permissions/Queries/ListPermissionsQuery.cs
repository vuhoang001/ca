using Api.Application;
using Application.Abstractions;
using MediatR;
using Shared.Abstractions;

namespace Application.Features.Permissions.Queries;

public sealed record ListPermissionsQuery : IRequest<IReadOnlyCollection<PermissionResponse>>;

public sealed class ListPermissionsQueryHandler(
    IPermissionRepository permissionRepository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ListPermissionsQuery, IReadOnlyCollection<PermissionResponse>>
{
    public async Task<IReadOnlyCollection<PermissionResponse>> Handle(ListPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionRepository.ListAsync(currentUserContext.TenantId, cancellationToken);
        return permissions
            .Select(permission => new PermissionResponse(
                        permission.Id,
                        permission.TenantId,
                        permission.Code,
                        permission.Name,
                        permission.Resource,
                        permission.Action,
                        permission.Description,
                        permission.IsActive))
            .ToArray();
    }
}