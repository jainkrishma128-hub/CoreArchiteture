using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.Application.Services;

public class RoleMenuService : IRoleMenuService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleMenuService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleMenuPermissionsDto?> GetRoleMenuPermissionsAsync(int roleId)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null) return null;

        var roleMenus = await _unitOfWork.RoleMenus.GetByRoleIdAsync(roleId);
        // Note: In a real app, you might want to join with Menus to get all available menus even if no permission entry exists.
        // For now, we'll return what's in the RoleMenus table.

        return new RoleMenuPermissionsDto
        {
            RoleId = role.Id,
            RoleName = role.RoleName,
            MenuPermissions = roleMenus.Select(rm => new RoleMenuItemDto
            {
                MenuId = rm.MenuId,
                MenuName = rm.Menu?.Name ?? "Unknown", // Assuming Menu navigation property is loaded
                CanCreate = rm.CanCreate,
                CanRead = rm.CanRead,
                CanUpdate = rm.CanUpdate,
                CanDelete = rm.CanDelete,
                CanExecute = rm.CanExecute
            }).ToList()
        };
    }

    public async Task<bool> UpdateRoleMenuPermissionsAsync(int roleId, List<RoleMenuItemDto> menuPermissions)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null) return false;

        var existingPermissions = await _unitOfWork.RoleMenus.GetByRoleIdAsync(roleId);
        
        // Optimization: Update existing, add new, delete missing instead of full wipe
        
        // 1. Delete removed permissions
        var toDelete = existingPermissions.Where(e => !menuPermissions.Any(m => m.MenuId == e.MenuId)).ToList();
        foreach (var item in toDelete)
        {
            await _unitOfWork.RoleMenus.DeleteAsync(item.Id);
        }

        // 2. Add or Update
        foreach (var permission in menuPermissions)
        {
            var existing = existingPermissions.FirstOrDefault(e => e.MenuId == permission.MenuId);
            
            if (existing != null)
            {
                // Update existing
                existing.CanCreate = permission.CanCreate;
                existing.CanRead = permission.CanRead;
                existing.CanUpdate = permission.CanUpdate;
                existing.CanDelete = permission.CanDelete;
                existing.CanExecute = permission.CanExecute;
            }
            else
            {
                // Add new
                var roleMenu = new RoleMenu
                {
                    RoleId = roleId,
                    MenuId = permission.MenuId,
                    CanCreate = permission.CanCreate,
                    CanRead = permission.CanRead,
                    CanUpdate = permission.CanUpdate,
                    CanDelete = permission.CanDelete,
                    CanExecute = permission.CanExecute,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.RoleMenus.AddAsync(roleMenu);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
