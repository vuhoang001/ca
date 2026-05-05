using Application.Abstractions;
using MediatR;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Products.Commands;

public sealed record DeleteProductCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.Id}' not found.");

        repository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
