using Application.Abstractions;
using Application.Common;
using Application.Features.Products.Dtos;
using MediatR;

namespace Application.Features.Products.Queries;

public sealed record ListProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null,
    string? SortBy = "name",
    bool SortDescending = false) : IRequest<PaginatedList<ProductDto>>;

public sealed class ListProductsQueryHandler(IProductRepository repository)
    : IRequestHandler<ListProductsQuery, PaginatedList<ProductDto>>
{
    public async Task<PaginatedList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var result = await repository.ListAsync(
            page, pageSize, request.Search, request.IsActive,
            request.SortBy, request.SortDescending, cancellationToken);

        return new PaginatedList<ProductDto>(
            result.Items.Select(ProductDto.FromEntity).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}
