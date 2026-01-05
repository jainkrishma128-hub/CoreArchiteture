using System.Net.Http.Json;
using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.IntegrationTests;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnSuccessAndData()
    {
        // Arrange
        // Seed some data
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            db.Products.Add(new Product { Name = "Test Product", Price = 10, Stock = 100, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<ProductDto>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(p => p.Name == "Test Product");
    }
}
