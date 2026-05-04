using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Permissions.Commands;

public sealed record DeletePermissionCommand(Guid PermissionId) : IRequest;

public sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}

public sealed class DeletePermissionCommandHandler(
    IPermissionRepository permissionRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePermissionCommand>
{
    public async Task<Unit> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken)
            ?? throw new NotFoundException("Permission not found.");

        permissionRepository.Remove(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("permission.deleted", nameof(Permission),
                                      permission.Id.ToString(), new { permission.Code }, "Success", cancellationToken);
        return Unit.Value;
    }
}