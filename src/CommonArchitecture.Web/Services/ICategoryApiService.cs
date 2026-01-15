using CommonArchitecture.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CommonArchitecture.Web.Services;

public interface ICategoryApiService
{
    Task<PaginatedResult<CategoryDto>> GetAllAsync(CategoryQueryParameters parameters);
    Task<IEnumerable<CategoryDto>> GetActiveAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto createDto);
    Task<bool> UpdateAsync(int id, UpdateCategoryDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<byte[]> ExportAsync();
    Task<bool> ImportAsync(IFormFile file);
}