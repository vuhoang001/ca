using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Roles.Commands;

public sealed record CreateRoleCommand(string Name, string? Description, Guid? TenantId = null, bool IsSystem = false)
    : IRequest<RoleResponse>;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoleCommand, RoleResponse>
{
    public async Task<RoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await roleRepository.ExistsByNormalizedNameAsync(request.Name.Trim().ToUpperInvariant(), request.TenantId, cancellationToken))
        {
            throw new ConflictException($"Role '{request.Name}' already exists.");
        }

        var role = new Role(request.TenantId, request.Name, request.Description, request.IsSystem);
        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("role.created", nameof(Role), role.Id.ToString(), new { role.Name }, "Success", cancellationToken);

        return new RoleResponse(role.Id, role.Name, role.Description, role.IsActive, role.IsSystem, role.TenantId);
    }
}
