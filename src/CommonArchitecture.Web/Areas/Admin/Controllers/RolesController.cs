using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Only Admin (1) can access
public class RolesController : Controller
{
    private readonly IRoleApiService _roleApiService;
    private readonly ILogger<RolesController> _logger;
    private readonly IValidator<CreateRoleDto> _createValidator;
    private readonly IValidator<UpdateRoleDto> _updateValidator;
    private readonly IToastNotification _toastNotification;

    public RolesController(
        IRoleApiService roleApiService, 
        ILogger<RolesController> logger,
        IValidator<CreateRoleDto> createValidator,
        IValidator<UpdateRoleDto> updateValidator,
        IToastNotification toastNotification)
    {
        _roleApiService = roleApiService;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _toastNotification = toastNotification;
    }

    // GET: Admin/Roles
    public IActionResult Index()
    {
        return View();
    }

    // GET: Admin/Roles/GetAll - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RoleQueryParameters parameters)
    {
        try
        {
            var result = await _roleApiService.GetAllAsync(parameters ?? new RoleQueryParameters());
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles");
            return Json(new { success = false, message = "Error fetching roles" });
        }
    }

    // GET: Admin/Roles/GetById/5 - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var role = await _roleApiService.GetByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found" });
            }

            return Json(new { success = true, data = role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role {RoleId}", id);
            return Json(new { success = false, message = "Error fetching role" });
        }
    }

    // POST: Admin/Roles/Create - AJAX endpoint
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto createDto)
    {
        // Server-side validation with FluentValidation
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
            var role = await _roleApiService.CreateAsync(createDto);
            _toastNotification.AddSuccessToastMessage("Role created successfully!");
            return Json(new { success = true, message = "Role created successfully!", data = role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            _toastNotification.AddErrorToastMessage("An error occurred while creating the role. Please try again.");
            return Json(new { success = false, message = "An error occurred while creating the role. Please try again." });
        }
    }

    // PUT: Admin/Roles/Edit/5 - AJAX endpoint
    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromBody] UpdateRoleDto updateDto)
    {
        // Server-side validation with FluentValidation
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
            var success = await _roleApiService.UpdateAsync(id, updateDto);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("Role not found");
                return Json(new { success = false, message = "Role not found" });
            }

            _toastNotification.AddSuccessToastMessage("Role updated successfully!");
            return Json(new { success = true, message = "Role updated successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while updating the role. Please try again.");
            return Json(new { success = false, message = "An error occurred while updating the role. Please try again." });
        }
    }

    // DELETE: Admin/Roles/Delete/5 - AJAX endpoint
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _roleApiService.DeleteAsync(id);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("Role not found");
                return Json(new { success = false, message = "Role not found" });
            }

            _toastNotification.AddSuccessToastMessage("Role deleted successfully!");
            return Json(new { success = true, message = "Role deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while deleting the role. Please try again.");
            return Json(new { success = false, message = "An error occurred while deleting the role. Please try again." });
        }
    }
}

