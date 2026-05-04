using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Permissions.Commands;

public sealed record CreatePermissionCommand(
    string Code,
    string Name,
    string Resource,
    string Action,
    string? Description)
    : IRequest<PermissionResponse>;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Resource).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreatePermissionCommandHandler(
    IPermissionRepository permissionRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePermissionCommand, PermissionResponse>
{
    public async Task<PermissionResponse> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        if (await permissionRepository.ExistsByCodeAsync(request.Code.Trim().ToLowerInvariant(), cancellationToken))
        {
            throw new ConflictException($"Permission '{request.Code}' already exists.");
        }

        var permission = new Permission(request.Code, request.Name, request.Resource, request.Action,
                                        request.Description);
        permissionRepository.Add(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("permission.created", nameof(Permission), permission.Id.ToString(),
                                      new { permission.Code }, "Success", cancellationToken);

        return new PermissionResponse(permission.Id, permission.Code, permission.Name, permission.Resource,
                                      permission.Action, permission.Description, permission.IsActive);
    }
}