using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Roles.Commands;

public sealed record AssignPermissionsToRoleCommand(Guid RoleId, IReadOnlyCollection<Guid> PermissionIds) : IRequest;

public sealed class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionIds).NotEmpty();
    }
}

public sealed class AssignPermissionsToRoleCommandHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignPermissionsToRoleCommand>
{
    public async Task<Unit> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, true, cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        var permissions = await permissionRepository.GetByIdsAsync(request.PermissionIds, cancellationToken);
        if (permissions.Count != request.PermissionIds.Count)
        {
            throw new NotFoundException("One or more permissions were not found.");
        }

        await roleRepository.AssignPermissionsAsync(role, permissions, currentUserContext.Email, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("role.permissions.assigned", nameof(Role), role.Id.ToString(),
                                      new { request.PermissionIds }, "Success", cancellationToken);
        return Unit.Value;
    }
}