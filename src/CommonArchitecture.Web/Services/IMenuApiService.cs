using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IMenuApiService
{
 Task<PaginatedResult<MenuDto>> GetAllAsync(MenuQueryParameters parameters);
 Task<MenuDto?> GetByIdAsync(int id);
 Task<MenuDto> CreateAsync(CreateMenuDto createDto);
 Task<bool> UpdateAsync(int id, UpdateMenuDto updateDto);
 Task<bool> DeleteAsync(int id);
}
