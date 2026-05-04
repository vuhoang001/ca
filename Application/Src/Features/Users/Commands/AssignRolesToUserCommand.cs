using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Users.Commands;

public sealed record AssignRolesToUserCommand(Guid UserId, IReadOnlyCollection<Guid> RoleIds) : IRequest;

public sealed class AssignRolesToUserCommandValidator : AbstractValidator<AssignRolesToUserCommand>
{
    public AssignRolesToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleIds).NotEmpty();
    }
}

public sealed class AssignRolesToUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignRolesToUserCommand>
{
    public async Task<Unit> Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, true, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var roles = await roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);
        if (roles.Count != request.RoleIds.Count)
        {
            throw new NotFoundException("One or more roles were not found.");
        }

        await userRepository.AssignRolesAsync(user, roles, currentUserContext.Email, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("user.roles.assigned", nameof(User),
                                      user.Id.ToString(), new { request.RoleIds }, "Success", cancellationToken);
        return Unit.Value;
    }
}