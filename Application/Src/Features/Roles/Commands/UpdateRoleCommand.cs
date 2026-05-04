using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Roles.Commands;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, string? Description, bool IsActive)
    : IRequest<RoleResponse>;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRoleCommand, RoleResponse>
{
    public async Task<RoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        role.Update(request.Name, request.Description, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("role.updated", nameof(Role), role.Id.ToString(),
                                      new { role.Name }, "Success", cancellationToken);

        return new RoleResponse(role.Id, role.Name, role.Description, role.IsActive, role.IsSystem, role.TenantId);
    }
}