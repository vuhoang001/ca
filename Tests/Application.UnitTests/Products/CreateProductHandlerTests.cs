using Application.Abstractions;
using Application.Features.Products.Commands;
using Domain.Entities;
using Moq;
using Shared.Abstractions;
using Shared.Exceptions;
using Xunit;

namespace Application.UnitTests.Products;

public sealed class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CreateProductCommandHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_WithNewSku_ShouldCreateAndReturnProduct()
    {
        _repoMock.Setup(r => r.GetBySkuAsync("SKU-001", default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var command = new CreateProductCommand("SKU-001", "Test Product", "Description", 9.99m, "USD", null);
        var result = await CreateHandler().Handle(command, default);

        Assert.Equal("SKU-001", result.Sku);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(9.99m, result.Price);
        Assert.True(result.IsActive);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingSku_ShouldThrowConflictException()
    {
        var existing = Product.Create("SKU-001", "Existing", null, 1m, "USD");
        _repoMock.Setup(r => r.GetBySkuAsync("SKU-001", default)).ReturnsAsync(existing);

        var command = new CreateProductCommand("SKU-001", "New Product", null, 9.99m, "USD", null);

        await Assert.ThrowsAsync<ConflictException>(() => CreateHandler().Handle(command, default));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_SkuShouldBeNormalizedToUpperCase()
    {
        _repoMock.Setup(r => r.GetBySkuAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Product?)null);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var command = new CreateProductCommand("sku-lower", "Test", null, 1m, "USD", null);
        var result = await CreateHandler().Handle(command, default);

        Assert.Equal("SKU-LOWER", result.Sku);
    }
}
