using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Application.Services;

public interface IProductService
{
 Task<PaginatedResult<ProductDto>> GetAllAsync(ProductQueryParameters parameters);
 Task<ProductDto?> GetByIdAsync(int id);
 Task<ProductDto> CreateAsync(CreateProductDto createDto, CancellationToken cancellationToken = default);
 Task<bool> UpdateAsync(int id, UpdateProductDto updateDto, CancellationToken cancellationToken = default);
 Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
 Task<byte[]> ExportAsync();
 Task<bool> ImportAsync(Stream fileStream);
}
