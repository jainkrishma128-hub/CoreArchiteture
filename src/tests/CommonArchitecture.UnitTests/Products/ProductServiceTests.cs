using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommonArchitecture.UnitTests.Products;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(x => x.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new ProductService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedProducts()
    {
        // Arrange
        var parameters = new ProductQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "",
            SortBy = "Id",
            SortOrder = "asc"
        };

        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 100 },
            new Product { Id = 2, Name = "Product 2", Price = 200 }
        };

        _productRepositoryMock.Setup(x => x.GetPagedAsync(
            parameters.SearchTerm,
            parameters.CategoryId,
            parameters.SortBy,
            parameters.SortOrder,
            parameters.PageNumber,
            parameters.PageSize))
            .ReturnsAsync(products);

        _productRepositoryMock.Setup(x => x.GetTotalCountAsync(parameters.SearchTerm, parameters.CategoryId))
            .ReturnsAsync(2);

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        
        var items = result.Items.ToList();
        items[0].Name.Should().Be("Product 1");
        items[1].Name.Should().Be("Product 2");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProductAndReturnDto()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Description",
            Price = 99.99m
        };

        _productRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => 
            {
                p.Id = 1; // Simulate DB assigning ID
                return p;
            });

        // Act
        var result = await _service.CreateAsync(createDto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(99.99m);
        
        _productRepositoryMock.Verify(x => x.AddAsync(It.Is<Product>(p => 
            p.Name == "New Product" && 
            p.Price == 99.99m)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
