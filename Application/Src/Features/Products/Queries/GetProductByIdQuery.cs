using Application.Abstractions;
using Application.Features.Products.Dtos;
using MediatR;
using Shared.Exceptions;

namespace Application.Features.Products.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public sealed class GetProductByIdQueryHandler(IProductRepository repository)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.Id}' not found.");

        return ProductDto.FromEntity(product);
    }
}
