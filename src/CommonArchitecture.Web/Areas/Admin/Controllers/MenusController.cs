using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Only Admin can access
public class MenusController : Controller
{
 private readonly IMenuApiService _menuApiService;
 private readonly ILogger<MenusController> _logger;
 private readonly IValidator<CreateMenuDto> _createValidator;
 private readonly IValidator<UpdateMenuDto> _updateValidator;
 private readonly IToastNotification _toastNotification;

 public MenusController(
 IMenuApiService menuApiService, 
 ILogger<MenusController> logger,
 IValidator<CreateMenuDto> createValidator,
 IValidator<UpdateMenuDto> updateValidator,
 IToastNotification toastNotification)
 {
 _menuApiService = menuApiService;
 _logger = logger;
 _createValidator = createValidator;
 _updateValidator = updateValidator;
 _toastNotification = toastNotification;
 }

 public IActionResult Index()
 {
 return View();
 }

 [HttpGet]
 public async Task<IActionResult> GetAll([FromQuery] MenuQueryParameters parameters)
 {
 try
 {
 var result = await _menuApiService.GetAllAsync(parameters);
 return Json(new { success = true, data = result });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching menus");
 return Json(new { success = false, message = "Error fetching menus" });
 }
 }

 [HttpGet]
 public async Task<IActionResult> GetById(int id)
 {
 try
 {
 var menu = await _menuApiService.GetByIdAsync(id);
 if (menu == null)
 {
 return Json(new { success = false, message = "Menu not found" });
 }

 return Json(new { success = true, data = menu });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching menu {MenuId}", id);
 return Json(new { success = false, message = "Error fetching menu" });
 }
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Create([FromBody] CreateMenuDto createDto)
 {
 var validationResult = await _createValidator.ValidateAsync(createDto);
 if (!validationResult.IsValid)
 {
 var errors = validationResult.Errors
 .GroupBy(e => e.PropertyName)
 .ToDictionary(
 g => g.Key,
 g => g.Select(e => e.ErrorMessage).ToArray()
 );
 _toastNotification.AddErrorToastMessage("Please fix the validation errors before submitting.");
 return Json(new { success = false, errors = errors });
 }

 try
 {
 var menu = await _menuApiService.CreateAsync(createDto);
 _toastNotification.AddSuccessToastMessage("Menu created successfully!");
 return Json(new { success = true, message = "Menu created successfully!", data = menu });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error creating menu");
 _toastNotification.AddErrorToastMessage("An error occurred while creating the menu. Please try again.");
 return Json(new { success = false, message = "An error occurred while creating the menu. Please try again." });
 }
 }

 [HttpPut]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(int id, [FromBody] UpdateMenuDto updateDto)
 {
 var validationResult = await _updateValidator.ValidateAsync(updateDto);
 if (!validationResult.IsValid)
 {
 var errors = validationResult.Errors
 .GroupBy(e => e.PropertyName)
 .ToDictionary(
 g => g.Key,
 g => g.Select(e => e.ErrorMessage).ToArray()
 );
 _toastNotification.AddErrorToastMessage("Please fix the validation errors before submitting.");
 return Json(new { success = false, errors = errors });
 }

 try
 {
 var success = await _menuApiService.UpdateAsync(id, updateDto);
 if (!success)
 {
 _toastNotification.AddWarningToastMessage("Menu not found");
 return Json(new { success = false, message = "Menu not found" });
 }

 _toastNotification.AddSuccessToastMessage("Menu updated successfully!");
 return Json(new { success = true, message = "Menu updated successfully!" });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error updating menu {MenuId}", id);
 _toastNotification.AddErrorToastMessage("An error occurred while updating the menu. Please try again.");
 return Json(new { success = false, message = "An error occurred while updating the menu. Please try again." });
 }
 }

 [HttpDelete]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Delete(int id)
 {
 try
 {
 var success = await _menuApiService.DeleteAsync(id);
 if (!success)
 {
 _toastNotification.AddWarningToastMessage("Menu not found");
 return Json(new { success = false, message = "Menu not found" });
 }

 _toastNotification.AddSuccessToastMessage("Menu deleted successfully!");
 return Json(new { success = true, message = "Menu deleted successfully!" });
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error deleting menu {MenuId}", id);
 _toastNotification.AddErrorToastMessage("An error occurred while deleting the menu. Please try again.");
 return Json(new { success = false, message = "An error occurred while deleting the menu. Please try again." });
 }
 }
}
