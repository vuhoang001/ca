using Application.Abstractions.Persistence;
using MediatR;

namespace Auth.Application.Auth.Commands.Register;

public record RegisterCommand : IRequest<Guid>
{
    public string Email { get; init; }
    public string Password { get; init; }
}

public class RegisterCommandHandler(IAuthRepository authRepository) : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var auth = new Domain.Entities.Auth(request.Email, request.Password, "false", Guid.NewGuid().ToString());
        await authRepository.AddAsync(auth, cancellationToken);
        await authRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return auth.Id;
    }
}