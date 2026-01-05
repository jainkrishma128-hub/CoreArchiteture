using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Application.Services;

public interface IMenuService
{
    Task<PaginatedResult<MenuDto>> GetAllAsync(MenuQueryParameters parameters);
    Task<MenuDto?> GetByIdAsync(int id);
    Task<MenuDto> CreateAsync(CreateMenuDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateMenuDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
