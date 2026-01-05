using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IRoleApiService
{
    Task<PaginatedResult<RoleDto>> GetAllAsync(RoleQueryParameters parameters);
    Task<RoleDto?> GetByIdAsync(int id);
    Task<RoleDto> CreateAsync(CreateRoleDto createDto);
    Task<bool> UpdateAsync(int id, UpdateRoleDto updateDto);
    Task<bool> DeleteAsync(int id);
}

