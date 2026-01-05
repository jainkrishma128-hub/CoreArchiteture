using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IRoleMenuApiService
{
 Task<RoleMenuPermissionsDto?> GetRolePermissionsAsync(int roleId);
 Task<bool> UpdateRolePermissionsAsync(int roleId, List<RoleMenuItemDto> menuPermissions);
}
