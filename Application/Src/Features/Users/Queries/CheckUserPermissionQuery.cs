using Api.Application;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Users.Queries;

public sealed record CheckUserPermissionQuery(Guid UserId, string PermissionCode) : IRequest<PermissionCheckResponse>;

public sealed class CheckUserPermissionQueryValidator : AbstractValidator<CheckUserPermissionQuery>
{
    public CheckUserPermissionQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PermissionCode).NotEmpty().MaximumLength(150);
    }
}

public sealed class CheckUserPermissionQueryHandler(IUserRepository userRepository)
    : IRequestHandler<CheckUserPermissionQuery, PermissionCheckResponse>
{
    public async Task<PermissionCheckResponse> Handle(CheckUserPermissionQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var permissions = await userRepository.GetPermissionCodesAsync(user.Id, cancellationToken);
        var granted = permissions.Contains(request.PermissionCode.Trim(), StringComparer.OrdinalIgnoreCase);
        return new PermissionCheckResponse(user.Id, request.PermissionCode.Trim(), granted);
    }
}