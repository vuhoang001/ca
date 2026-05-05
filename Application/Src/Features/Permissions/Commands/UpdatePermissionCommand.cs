using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Permissions.Commands;

public sealed record UpdatePermissionCommand(
    Guid PermissionId,
    string Name,
    string Resource,
    string Action,
    string? Description,
    bool IsActive)
    : IRequest<PermissionResponse>;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Resource).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdatePermissionCommandHandler(
    IPermissionRepository permissionRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePermissionCommand, PermissionResponse>
{
    public async Task<PermissionResponse> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken)
            ?? throw new NotFoundException("Permission not found.");

        permission.Update(request.Name, request.Resource, request.Action, request.Description, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("permission.updated", nameof(Permission), permission.Id.ToString(),
                                      new { permission.Code }, "Success", cancellationToken);

        return new PermissionResponse(permission.Id, permission.TenantId, permission.Code, permission.Name,
                                      permission.Resource, permission.Action, permission.Description,
                                      permission.IsActive);
    }
}