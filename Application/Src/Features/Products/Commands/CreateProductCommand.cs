using Application.Abstractions;
using Application.Features.Products.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Products.Commands;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    Guid? TenantId) : IRequest<ProductDto>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"Product with SKU '{request.Sku.ToUpperInvariant()}' already exists.");

        var product = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.TenantId);

        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.FromEntity(product);
    }
}
