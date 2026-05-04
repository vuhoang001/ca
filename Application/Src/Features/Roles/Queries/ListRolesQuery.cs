using Api.Application;
using Application.Abstractions;
using MediatR;

namespace Application.Features.Roles.Queries;

public sealed record ListRolesQuery(Guid? TenantId = null) : IRequest<IReadOnlyCollection<RoleResponse>>;

public sealed class ListRolesQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<ListRolesQuery, IReadOnlyCollection<RoleResponse>>
{
    public async Task<IReadOnlyCollection<RoleResponse>> Handle(ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await roleRepository.ListAsync(request.TenantId, cancellationToken);
        return roles
            .Select(role => new RoleResponse(role.Id, role.Name, role.Description, role.IsActive, role.IsSystem,
                                             role.TenantId))
            .ToArray();
    }
}