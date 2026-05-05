using System.Net;
using System.Net.Http.Json;
using Integration.Tests.Fixtures;
using Xunit;

namespace Integration.Tests.Products;

public sealed class ProductEndpointTests : IClassFixture<IntegrationTestBase>
{
    private readonly HttpClient _client;

    public ProductEndpointTests(IntegrationTestBase fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        var request = new
        {
            Sku = $"TEST-{Guid.NewGuid():N}"[..12],
            Name = "Integration Test Product",
            Description = "Created in integration test",
            Price = 19.99m,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ShouldReturnConflict()
    {
        var sku = $"DUP-{Guid.NewGuid():N}"[..10];
        var request = new { Sku = sku, Name = "Product", Price = 1m, Currency = "USD" };

        await _client.PostAsJsonAsync("/api/v1/products", request);
        var duplicate = await _client.PostAsJsonAsync("/api/v1/products", request);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/products/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldReturnOk()
    {
        var sku = $"UPD-{Guid.NewGuid():N}"[..10];
        var createResp = await _client.PostAsJsonAsync("/api/v1/products",
            new { Sku = sku, Name = "Original", Price = 1m, Currency = "USD" });
        createResp.EnsureSuccessStatusCode();

        var created = await createResp.Content.ReadFromJsonAsync<dynamic>();
        string id = created!.data.id.ToString();

        var updateResp = await _client.PutAsJsonAsync($"/api/v1/products/{id}",
            new { Name = "Updated", Description = "New desc", Price = 2m, Currency = "EUR" });

        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent()
    {
        var sku = $"DEL-{Guid.NewGuid():N}"[..10];
        var createResp = await _client.PostAsJsonAsync("/api/v1/products",
            new { Sku = sku, Name = "To Delete", Price = 1m, Currency = "USD" });
        createResp.EnsureSuccessStatusCode();

        var created = await createResp.Content.ReadFromJsonAsync<dynamic>();
        string id = created!.data.id.ToString();

        var deleteResp = await _client.DeleteAsync($"/api/v1/products/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }
}
