using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Roles.Commands;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRoleCommand>
{
    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        roleRepository.Remove(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("role.deleted", nameof(Role), role.Id.ToString(), new { role.Name }, "Success",
                                      cancellationToken);
        return Unit.Value;
    }
}