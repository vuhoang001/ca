using Domain.Entities;

namespace Application.Features.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive,
    Guid? TenantId,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy)
{
    public static ProductDto FromEntity(Product p) =>
        new(p.Id, p.Sku, p.Name, p.Description, p.Price, p.Currency,
            p.IsActive, p.TenantId, p.CreatedAtUtc, p.CreatedBy,
            p.LastModifiedAtUtc, p.LastModifiedBy);
}
