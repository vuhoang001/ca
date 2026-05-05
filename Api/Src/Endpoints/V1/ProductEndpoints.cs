using Application.Features.Products.Commands;
using Application.Features.Products.Queries;
using MediatR;
using Shared.Results;

namespace Api.Endpoints.V1;

public sealed class ProductEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (
            [AsParameters] ListProductsQuery query,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(query, ct);
            return Results.Ok(new ApiEnvelope<object>(result));
        })
        .RequireAuthorization(p => p.RequireRole("masterdata-reader", "masterdata-writer", "admin"))
        .RequireRateLimiting("default");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetProductByIdQuery(id), ct);
            return Results.Ok(new ApiEnvelope<object>(result));
        })
        .RequireAuthorization(p => p.RequireRole("masterdata-reader", "masterdata-writer", "admin"))
        .RequireRateLimiting("default");

        group.MapPost("/", async (
            CreateProductCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/v1/products/{result.Id}", new ApiEnvelope<object>(result));
        })
        .RequireAuthorization(p => p.RequireRole("masterdata-writer", "admin"))
        .RequireRateLimiting("default");

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Currency);
            var result = await mediator.Send(command, ct);
            return Results.Ok(new ApiEnvelope<object>(result));
        })
        .RequireAuthorization(p => p.RequireRole("masterdata-writer", "admin"))
        .RequireRateLimiting("default");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteProductCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(p => p.RequireRole("masterdata-writer", "admin"))
        .RequireRateLimiting("default");
    }
}

public sealed record UpdateProductRequest(string Name, string? Description, decimal Price, string Currency);
