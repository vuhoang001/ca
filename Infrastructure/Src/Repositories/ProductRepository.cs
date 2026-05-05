using Application.Abstractions;
using Application.Common;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) =>
        dbContext.Products.FirstOrDefaultAsync(
            p => p.Sku == sku.Trim().ToUpperInvariant(), ct);

    public async Task<PaginatedList<Product>> ListAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        bool sortDescending,
        CancellationToken ct = default)
    {
        var query = dbContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        query = sortBy?.ToLowerInvariant() switch
        {
            "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "sku" => sortDescending ? query.OrderByDescending(p => p.Sku) : query.OrderBy(p => p.Sku),
            "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderBy(p => p.CreatedAtUtc),
            _ => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PaginatedList<Product>(items, totalCount, page, pageSize);
    }

    public Task AddAsync(Product product, CancellationToken ct = default) =>
        dbContext.Products.AddAsync(product, ct).AsTask();

    public void Update(Product product) => dbContext.Products.Update(product);

    public void Remove(Product product) => dbContext.Products.Remove(product);
}
