using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Application.Services;

public interface IRoleMenuService
{
    Task<RoleMenuPermissionsDto?> GetRoleMenuPermissionsAsync(int roleId);
    Task<bool> UpdateRoleMenuPermissionsAsync(int roleId, List<RoleMenuItemDto> menuPermissions);
}
