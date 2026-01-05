using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Application.Services;

public interface IRoleService
{
 Task<PaginatedResult<RoleDto>> GetAllAsync(RoleQueryParameters parameters);
 Task<RoleDto?> GetByIdAsync(int id);
 Task<RoleDto> CreateAsync(string roleName, CancellationToken cancellationToken = default);
 Task<bool> UpdateAsync(int id, string roleName, CancellationToken cancellationToken = default);
 Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
