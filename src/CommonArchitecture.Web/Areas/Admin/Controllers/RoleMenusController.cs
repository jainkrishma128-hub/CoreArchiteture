using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Only Admin can access
public class RoleMenusController : Controller
{
 private readonly IRoleMenuApiService _roleMenuApiService;
 private readonly IRoleApiService _roleApiService;
 private readonly IMenuApiService _menuApiService;
 private readonly ILogger<RoleMenusController> _logger;
 private readonly IToastNotification _toastNotification;

 public RoleMenusController(
 IRoleMenuApiService roleMenuApiService,
 IRoleApiService roleApiService,
 IMenuApiService menuApiService,
 ILogger<RoleMenusController> logger,
 IToastNotification toastNotification)
 {
 _roleMenuApiService = roleMenuApiService;
 _roleApiService = roleApiService;
 _menuApiService = menuApiService;
 _logger = logger;
 _toastNotification = toastNotification;
 }

 public IActionResult Index()
 {
 return View();
 }

 [HttpGet]
 public async Task<IActionResult> GetAllRoles()
 {
 try
 {
 var result = await _roleApiService.GetAllAsync(new RoleQueryParameters { PageSize = 100 });
 return Json(new { success = true, data = result.Items });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching roles");
 return Json(new { success = false, message = "Error fetching roles" });
 }
 }

 [HttpGet]
 public async Task<IActionResult> GetAllMenus()
 {
 try
 {
 var result = await _menuApiService.GetAllAsync(new MenuQueryParameters { PageSize = 1000 });
 return Json(new { success = true, data = result.Items });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching menus");
 return Json(new { success = false, message = "Error fetching menus" });
 }
 }

 [HttpGet]
 public async Task<IActionResult> GetRolePermissions(int roleId)
 {
 try
 {
 var result = await _roleMenuApiService.GetRolePermissionsAsync(roleId);
 if (result == null)
 {
 return Json(new { success = false, message = "Role not found" });
 }

 return Json(new { success = true, data = result });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching role permissions for role {RoleId}", roleId);
 return Json(new { success = false, message = "Error fetching role permissions" });
 }
 }

 [HttpPut]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> UpdatePermissions(int roleId, [FromBody] List<RoleMenuItemDto> menuPermissions)
 {
 try
 {
 if (menuPermissions == null || menuPermissions.Count == 0)
 {
 return Json(new { success = false, message = "At least one menu must be selected" });
 }

 var success = await _roleMenuApiService.UpdateRolePermissionsAsync(roleId, menuPermissions);
 if (!success)
 {
 _toastNotification.AddErrorToastMessage("Failed to update role permissions");
 return Json(new { success = false, message = "Failed to update role permissions" });
 }

 _toastNotification.AddSuccessToastMessage("Role permissions updated successfully!");
 return Json(new { success = true, message = "Role permissions updated successfully!" });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error updating role permissions for role {RoleId}", roleId);
 _toastNotification.AddErrorToastMessage("An error occurred while updating role permissions");
 return Json(new { success = false, message = "An error occurred while updating role permissions" });
 }
 }
}
