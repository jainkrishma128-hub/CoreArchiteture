using CommonArchitecture.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CommonArchitecture.Web.Services;

public interface IProductApiService
{
    Task<PaginatedResult<ProductDto>> GetAllAsync(ProductQueryParameters parameters);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto createDto);
    Task<bool> UpdateAsync(int id, UpdateProductDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<byte[]> ExportAsync();
    Task<bool> ImportAsync(IFormFile file);
}
