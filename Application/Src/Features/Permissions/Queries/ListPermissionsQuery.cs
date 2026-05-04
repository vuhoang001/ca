using Api.Application;
using MediatR;

namespace Application.Features.Permissions.Queries;

public sealed record ListPermissionsQuery : IRequest<IReadOnlyCollection<PermissionResponse>>;

public sealed class ListPermissionsQueryHandler(IPermissionRepository permissionRepository)
    : IRequestHandler<ListPermissionsQuery, IReadOnlyCollection<PermissionResponse>>
{
    public async Task<IReadOnlyCollection<PermissionResponse>> Handle(ListPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionRepository.ListAsync(cancellationToken);
        return permissions
            .Select(permission => new PermissionResponse(
                        permission.Id,
                        permission.Code,
                        permission.Name,
                        permission.Resource,
                        permission.Action,
                        permission.Description,
                        permission.IsActive))
            .ToArray();
    }
}