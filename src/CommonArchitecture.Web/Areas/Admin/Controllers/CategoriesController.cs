using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1, 2)] // Admin (1) and Category Manager (2) can access
public class CategoriesController : Controller
{
    private readonly ICategoryApiService _categoryApiService;
    private readonly ILogger<CategoriesController> _logger;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updateValidator;
    private readonly IToastNotification _toastNotification;

    public CategoriesController(
        ICategoryApiService categoryApiService, 
        ILogger<CategoriesController> logger,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<UpdateCategoryDto> updateValidator,
        IToastNotification toastNotification)
    {
        _categoryApiService = categoryApiService;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _toastNotification = toastNotification;
    }

    // GET: Admin/Categories
    public IActionResult Index()
    {
        return View();
    }

    // GET: Admin/Categories/GetAll - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParameters parameters)
    {
        try
        {
            var result = await _categoryApiService.GetAllAsync(parameters);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return Json(new { success = false, message = "Error fetching categories" });
        }
    }

    // GET: Admin/Categories/GetById/5 - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var category = await _categoryApiService.GetByIdAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Category not found" });
            }

            return Json(new { success = true, data = category });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category {CategoryId}", id);
            return Json(new { success = false, message = "Error fetching category" });
        }
    }

    // POST: Admin/Categories/Create - AJAX endpoint
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto createDto)
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
            var category = await _categoryApiService.CreateAsync(createDto);
            _toastNotification.AddSuccessToastMessage("Category created successfully!");
            return Json(new { success = true, message = "Category created successfully!", data = category });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            _toastNotification.AddErrorToastMessage("An error occurred while creating the category. Please try again.");
            return Json(new { success = false, message = "An error occurred while creating the category. Please try again." });
        }
    }

    // PUT: Admin/Categories/Edit/5 - AJAX endpoint
    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromBody] UpdateCategoryDto updateDto)
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
            var success = await _categoryApiService.UpdateAsync(id, updateDto);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("Category not found");
                return Json(new { success = false, message = "Category not found" });
            }

            _toastNotification.AddSuccessToastMessage("Category updated successfully!");
            return Json(new { success = true, message = "Category updated successfully!" });
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning(ex, "Unauthorized request when updating category {CategoryId}", id);
            return Unauthorized(new { success = false, message = "Session expired. Please login again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while updating the category. Please try again.");
            return Json(new { success = false, message = "An error occurred while updating the category. Please try again." });
        }
    }

    // DELETE: Admin/Categories/Delete/5 - AJAX endpoint
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _categoryApiService.DeleteAsync(id);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("Category not found");
                return Json(new { success = false, message = "Category not found" });
            }

            _toastNotification.AddSuccessToastMessage("Category deleted successfully!");
            return Json(new { success = true, message = "Category deleted successfully!" });
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning(ex, "Unauthorized request when deleting category {CategoryId}", id);
            return Unauthorized(new { success = false, message = "Session expired. Please login again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while deleting the category. Please try again.");
            return Json(new { success = false, message = "An error occurred while deleting the category. Please try again." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var fileContent = await _categoryApiService.ExportAsync();
        if (fileContent.Length == 0)
        {
             _toastNotification.AddErrorToastMessage("Export failed.");
             return RedirectToAction("Index");
        }
        return File(fileContent, "text/csv", $"categories_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
             return Json(new { success = false, message = "Please select a file." });
        }

        var success = await _categoryApiService.ImportAsync(file);
        if (success)
        {
             _toastNotification.AddSuccessToastMessage("Categories imported successfully.");
             return Json(new { success = true });
        }
        else
        {
             return Json(new { success = false, message = "Import failed. Use valid CSV format." });
        }
    }

    // GET: Admin/Categories/DownloadTemplate
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        var csvHeader = "Name,Description,IsActive\n";
        var fileContent = System.Text.Encoding.UTF8.GetBytes(csvHeader);
        return File(fileContent, "text/csv", "category_import_template.csv");
    }
}