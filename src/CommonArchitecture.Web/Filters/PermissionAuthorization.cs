using CommonArchitecture.Core.Enums;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Filters;

public class HasPermissionAttribute : TypeFilterAttribute
{
    public HasPermissionAttribute(string menuName, PermissionType permission) 
        : base(typeof(PermissionFilter))
    {
        Arguments = new object[] { menuName, permission };
    }
}

public class PermissionFilter : IAsyncAuthorizationFilter
{
    private readonly string _menuName;
    private readonly PermissionType _permission;
    private readonly IRoleMenuApiService _roleMenuService;

    public PermissionFilter(string menuName, PermissionType permission, IRoleMenuApiService roleMenuService)
    {
        _menuName = menuName;
        _permission = permission;
        _roleMenuService = roleMenuService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. Get User Role from Session
        var roleIdStr = context.HttpContext.Session.GetString("UserRoleId");
        
        // If not logged in or no role, let the other auth filters handle it or redirect
        if (string.IsNullOrEmpty(roleIdStr) || !int.TryParse(roleIdStr, out int roleId))
        {
             // Check if user is authenticated at all?
             // If not, redirect to login
             context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
             return;
        }

        // 2. Get Permissions for this Role
        // Optimization: Cache permissions in session to avoid DB calls on every request
        var cacheKey = $"RolePermissions_{roleId}";
        var cachedPermissions = context.HttpContext.Session.GetString(cacheKey);
        RoleMenuPermissionsDto permissions = null;

        if (!string.IsNullOrEmpty(cachedPermissions))
        {
            permissions = JsonSerializer.Deserialize<RoleMenuPermissionsDto>(cachedPermissions);
        }

        if (permissions == null)
        {
            permissions = await _roleMenuService.GetRolePermissionsAsync(roleId);
            if (permissions != null)
            {
                context.HttpContext.Session.SetString(cacheKey, JsonSerializer.Serialize(permissions));
            }
        }

        if (permissions == null)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Error", new { area = "" });
            return;
        }

        // 3. Find the specific menu permission
        var menuPerm = permissions.MenuPermissions
            .FirstOrDefault(m => m.MenuName.Equals(_menuName, StringComparison.OrdinalIgnoreCase));

        // 4. Validate Logic
        bool isAuthorized = false;
        if (menuPerm != null)
        {
            isAuthorized = _permission switch
            {
                PermissionType.View => menuPerm.CanRead,
                PermissionType.Create => menuPerm.CanCreate,
                PermissionType.Edit => menuPerm.CanUpdate,
                PermissionType.Delete => menuPerm.CanDelete,
                PermissionType.Execute => menuPerm.CanExecute,
                _ => false
            };
        }

        if (!isAuthorized)
        {
            // User is logged in but lacks specific permission -> Access Denied
            // We can redirect to a specific "Access Denied" page or show specific error
            context.Result = new RedirectToActionResult("AccessDenied", "Error", new { area = "" }); 
        }
    }
}
