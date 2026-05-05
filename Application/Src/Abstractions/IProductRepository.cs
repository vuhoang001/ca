using Application.Common;
using Domain.Entities;

namespace Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<PaginatedList<Product>> ListAsync(int page, int pageSize, string? search, bool? isActive, string? sortBy, bool sortDescending, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Remove(Product product);
}
