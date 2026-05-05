using Domain.Entities;
using Xunit;

namespace Domain.UnitTests.Products;

public sealed class ProductEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnActiveProduct()
    {
        var product = Product.Create("sku-001", "Test Product", "A description", 9.99m, "usd");

        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("A description", product.Description);
        Assert.Equal(9.99m, product.Price);
        Assert.Equal("USD", product.Currency);
        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void Create_SkuAndCurrencyShouldBeUpperCase()
    {
        var product = Product.Create("  sku-001  ", "Name", null, 1m, "  eur  ");

        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal("EUR", product.Currency);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var product = Product.Create("SKU-001", "Test", null, 1m, "USD");
        product.Deactivate();
        Assert.False(product.IsActive);
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveToTrue()
    {
        var product = Product.Create("SKU-001", "Test", null, 1m, "USD");
        product.Deactivate();
        product.Activate();
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Update_ShouldChangeNameDescriptionPriceAndCurrency()
    {
        var product = Product.Create("SKU-001", "Old Name", null, 1m, "USD");

        product.Update("New Name", "New desc", 2.50m, "eur");

        Assert.Equal("New Name", product.Name);
        Assert.Equal("New desc", product.Description);
        Assert.Equal(2.50m, product.Price);
        Assert.Equal("EUR", product.Currency);
    }

    [Fact]
    public void Create_WithTenantId_ShouldPersistTenantId()
    {
        var tenantId = Guid.NewGuid();
        var product = Product.Create("SKU-001", "Test", null, 1m, "USD", tenantId);
        Assert.Equal(tenantId, product.TenantId);
    }
}
