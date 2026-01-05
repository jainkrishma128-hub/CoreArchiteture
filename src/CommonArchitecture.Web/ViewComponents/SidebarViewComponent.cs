using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.Web.ViewComponents;

public class SidebarViewComponent : ViewComponent
{
    private readonly IMenuApiService _menuService;
    private readonly IRoleMenuApiService _roleMenuService;

    public SidebarViewComponent(IMenuApiService menuService, IRoleMenuApiService roleMenuService)
    {
        _menuService = menuService;
        _roleMenuService = roleMenuService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var roleIdStr = HttpContext.Session.GetString("UserRoleId");
        
        // Default to no access if not logged in
        if (string.IsNullOrEmpty(roleIdStr) || !int.TryParse(roleIdStr, out int roleId))
        {
            return View(new List<SidebarMenuViewModel>());
        }

        // 1. Get All Menus
        var menuParams = new MenuQueryParameters { PageSize = 1000, SortBy = "DisplayOrder" };
        var menuResult = await _menuService.GetAllAsync(menuParams);
        
        if (menuResult?.Items == null || !menuResult.Items.Any())
        {
            return View(new List<SidebarMenuViewModel>());
        }

        var allMenus = menuResult.Items;

        // 2. Get User Permissions
        var permissions = await _roleMenuService.GetRolePermissionsAsync(roleId);
        var allowedMenuIds = new HashSet<int>();

        if (permissions != null && permissions.MenuPermissions != null)
        {
            allowedMenuIds = permissions.MenuPermissions
                .Where(p => p.CanRead)
                .Select(p => p.MenuId)
                .ToHashSet();
        }

        // 3. Filter and Build Tree
        var sidebarMenus = new List<SidebarMenuViewModel>();

        // First pass: Create ViewModels for allowed menus
        var menuMap = new Dictionary<int, SidebarMenuViewModel>();
        foreach (var menu in allMenus)
        {
            // If Admin (1), show everything? Or stick to permissions?
            // Sticking to permissions is better, ensuring seed data was correct.
            // However, for safety during dev, if Role is Admin(1), we can allow all.
            if (allowedMenuIds.Contains(menu.Id))
            {
                menuMap[menu.Id] = new SidebarMenuViewModel
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Url = menu.Url,
                    Icon = menu.Icon,
                    ParentMenuId = menu.ParentMenuId,
                    DisplayOrder = menu.DisplayOrder,
                    SubMenus = new List<SidebarMenuViewModel>()
                };
            }
        }

        // Second pass: Organize into tree
        foreach (var menuVm in menuMap.Values.OrderBy(m => m.DisplayOrder))
        {
            if (menuVm.ParentMenuId.HasValue && menuMap.ContainsKey(menuVm.ParentMenuId.Value))
            {
                // Add to parent
                menuMap[menuVm.ParentMenuId.Value].SubMenus.Add(menuVm);
            }
            else
            {
                // Top level
                sidebarMenus.Add(menuVm);
            }
        }

        // Set Active State
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();
        var currentUrl = $"/Admin/{currentController}"; // Approximation

        SetActiveState(sidebarMenus, currentUrl, currentController);

        return View(sidebarMenus);
    }

    private bool SetActiveState(List<SidebarMenuViewModel> menus, string currentUrl, string? currentController)
    {
        bool anyActive = false;
        foreach (var menu in menus)
        {
            // Check direct match or controller match
            // e.g. Menu Url: "/Admin/Products", Current: "/Admin/Products"
            // e.g. Menu Url: "/Admin/Products", Current: "/Admin/Products/Edit/1"
            
            bool isDirectMatch = string.Equals(menu.Url, currentUrl, StringComparison.OrdinalIgnoreCase);
            bool isControllerMatch = !string.IsNullOrEmpty(currentController) && 
                                   menu.Url.Contains(currentController, StringComparison.OrdinalIgnoreCase);

            if (isDirectMatch || (isControllerMatch && menu.SubMenus.Count == 0))
            {
                menu.IsActive = true;
                anyActive = true;
            }

            // Check children
            if (menu.SubMenus.Any())
            {
                bool childActive = SetActiveState(menu.SubMenus, currentUrl, currentController);
                if (childActive)
                {
                    menu.IsActive = true; // Highlight parent if child is active
                    // menu.IsExpanded = true; // If we supported expansion states
                    anyActive = true;
                }
            }
        }
        return anyActive;
    }
}

public class SidebarMenuViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int? ParentMenuId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<SidebarMenuViewModel> SubMenus { get; set; } = new();
}
