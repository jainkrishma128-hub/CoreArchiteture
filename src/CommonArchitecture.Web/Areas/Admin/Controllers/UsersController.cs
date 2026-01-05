using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Only Admin (1) can access
public class UsersController : Controller
{
    private readonly IUserApiService _userApiService;
    private readonly IRoleApiService _roleApiService;
    private readonly ILogger<UsersController> _logger;
    private readonly IValidator<CreateUserDto> _createValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly IToastNotification _toastNotification;

    public UsersController(
        IUserApiService userApiService,
        IRoleApiService roleApiService,
        ILogger<UsersController> logger,
        IValidator<CreateUserDto> createValidator,
        IValidator<UpdateUserDto> updateValidator,
        IToastNotification toastNotification)
    {
        _userApiService = userApiService;
        _roleApiService = roleApiService;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _toastNotification = toastNotification;
    }

    // GET: Admin/Users
    public IActionResult Index()
    {
        return View();
    }

    // GET: Admin/Users/GetAllRoles - AJAX endpoint for roles dropdown
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

    // GET: Admin/Users/GetAll - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryParameters parameters)
    {
        try
        {
            var result = await _userApiService.GetAllAsync(parameters ?? new UserQueryParameters());
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return Json(new { success = false, message = "Error fetching users" });
        }
    }

    // GET: Admin/Users/GetById/5 - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = await _userApiService.GetByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            return Json(new { success = true, data = user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            return Json(new { success = false, message = "Error fetching user" });
        }
    }

    // POST: Admin/Users/Create - AJAX endpoint
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateUserFormDto formDto)
    {
        var createDto = new CreateUserDto
        {
            Name = formDto.Name,
            Email = formDto.Email,
            Mobile = formDto.Mobile,
            RoleId = formDto.RoleId,
            ProfileImagePath = null // Will be set after file upload
        };

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
            var user = await _userApiService.CreateAsync(createDto, formDto.ProfileImage);
            _toastNotification.AddSuccessToastMessage("User created successfully!");
            return Json(new { success = true, message = "User created successfully!", data = user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            _toastNotification.AddErrorToastMessage("An error occurred while creating the user. Please try again.");
            return Json(new { success = false, message = "An error occurred while creating the user. Please try again." });
        }
    }

    // PUT: Admin/Users/Edit/5 - AJAX endpoint
    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromForm] UpdateUserFormDto formDto)
    {
        var updateDto = new UpdateUserDto
        {
            Name = formDto.Name,
            Email = formDto.Email,
            Mobile = formDto.Mobile,
            RoleId = formDto.RoleId,
            ProfileImagePath = formDto.ExistingProfileImagePath
        };

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
            var success = await _userApiService.UpdateAsync(id, updateDto, formDto.ProfileImage);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("User not found");
                return Json(new { success = false, message = "User not found" });
            }

            _toastNotification.AddSuccessToastMessage("User updated successfully!");
            return Json(new { success = true, message = "User updated successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while updating the user. Please try again.");
            return Json(new { success = false, message = "An error occurred while updating the user. Please try again." });
        }
    }

    // DELETE: Admin/Users/Delete/5 - AJAX endpoint
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _userApiService.DeleteAsync(id);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("User not found");
                return Json(new { success = false, message = "User not found" });
            }

            _toastNotification.AddSuccessToastMessage("User deleted successfully!");
            return Json(new { success = true, message = "User deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while deleting the user. Please try again.");
            return Json(new { success = false, message = "An error occurred while deleting the user. Please try again." });
        }
    }
}

// DTOs for form data binding
public class CreateUserFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

public class UpdateUserFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public string? ExistingProfileImagePath { get; set; }
}

