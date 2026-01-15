using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace CommonArchitecture.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<ProductDto>> GetAllAsync(ProductQueryParameters parameters)
    {
        var items = await _unitOfWork.Products.GetPagedAsync(parameters.SearchTerm, parameters.CategoryId, parameters.SortBy, parameters.SortOrder, parameters.PageNumber, parameters.PageSize);
        var total = await _unitOfWork.Products.GetTotalCountAsync(parameters.SearchTerm, parameters.CategoryId);


        // Category is already loaded via Include in repository - no N+1 query issue
        // Fetch stock for all items
        var dtosList = new List<ProductDto>();
        foreach (var p in items)
        {
            var stock = await _unitOfWork.InventoryTransactions.GetCurrentStockAsync(p.Id);
            dtosList.Add(new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                Stock = stock
            });
        }

        return new PaginatedResult<ProductDto>
        {
            Items = dtosList,
            TotalCount = total,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return null;

        var stock = await _unitOfWork.InventoryTransactions.GetCurrentStockAsync(product.Id);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "Uncategorized",
            Stock = stock
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createDto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            CategoryId = createDto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with Category to avoid N+1 query
        var productWithCategory = await _unitOfWork.Products.GetByIdAsync(createdProduct.Id);

        return new ProductDto
        {
            Id = productWithCategory!.Id,
            Name = productWithCategory.Name,
            Description = productWithCategory.Description,
            Price = productWithCategory.Price,
            CategoryId = productWithCategory.CategoryId,
            CategoryName = productWithCategory.Category?.Name ?? "Uncategorized"
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto updateDto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        product.Name = updateDto.Name;
        product.Description = updateDto.Description;
        product.Price = updateDto.Price;
        product.CategoryId = updateDto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return false;

        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<byte[]> ExportAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var dtos = products.Select(p => new
        {
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Category = p.Category?.Name ?? "Uncategorized"
        });

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        await csv.WriteRecordsAsync(dtos);
        await writer.FlushAsync();
        
        return memoryStream.ToArray();
    }

    public async Task<bool> ImportAsync(Stream fileStream)
    {
        try
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<dynamic>().ToList();
            if (!records.Any()) return false;

            var products = new List<Product>();
            foreach (var record in records)
            {
                int categoryId = 1;
                if (int.TryParse(record.CategoryId?.ToString(), out int catId))
                {
                    categoryId = catId;
                }

                var product = new Product
                {
                    Name = record.Name,
                    Description = record.Description,
                    Price = decimal.Parse(record.Price),
                    CategoryId = categoryId,
                    CreatedAt = DateTime.UtcNow
                };
                products.Add(product);
            }

            await _unitOfWork.Products.BulkAddAsync(products);
            await _unitOfWork.SaveChangesAsync();
     
            return true;
        }
        catch
        {
          return false;
        }
    }
}

