using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1, 2)] // Admin (1) and Product Manager (2) can access
public class ProductsController : Controller
{
    private readonly IProductApiService _productApiService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly IToastNotification _toastNotification;

    public ProductsController(
        IProductApiService productApiService, 
        ILogger<ProductsController> logger,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        IToastNotification toastNotification)
    {
        _productApiService = productApiService;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _toastNotification = toastNotification;
    }

    // GET: Admin/Products
    public IActionResult Index()
    {
        return View();
    }

    // GET: Admin/Products/GetAll - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters parameters)
    {
        try
        {
            var result = await _productApiService.GetAllAsync(parameters);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            return Json(new { success = false, message = "Error fetching products" });
        }
    }

    // GET: Admin/Products/GetById/5 - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productApiService.GetByIdAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            return Json(new { success = true, data = product });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", id);
            return Json(new { success = false, message = "Error fetching product" });
        }
    }

    // POST: Admin/Products/Create - AJAX endpoint
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createDto)
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
            var product = await _productApiService.CreateAsync(createDto);
            _toastNotification.AddSuccessToastMessage("Product created successfully!");
            return Json(new { success = true, message = "Product created successfully!", data = product });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            _toastNotification.AddErrorToastMessage("An error occurred while creating the product. Please try again.");
            return Json(new { success = false, message = "An error occurred while creating the product. Please try again." });
        }
    }

    // PUT: Admin/Products/Edit/5 - AJAX endpoint
    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromBody] UpdateProductDto updateDto)
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
            var success = await _productApiService.UpdateAsync(id, updateDto);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("Product not found");
                return Json(new { success = false, message = "Product not found" });
            }

            _toastNotification.AddSuccessToastMessage("Product updated successfully!");
            return Json(new { success = true, message = "Product updated successfully!" });
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning(ex, "Unauthorized request when updating product {ProductId}", id);
            return Unauthorized(new { success = false, message = "Session expired. Please login again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while updating the product. Please try again.");
            return Json(new { success = false, message = "An error occurred while updating the product. Please try again." });
        }
    }

    // DELETE: Admin/Products/Delete/5 - AJAX endpoint
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _productApiService.DeleteAsync(id);
            if (!success)
            {
                _toastNotification.AddWarningToastMessage("Product not found");
                return Json(new { success = false, message = "Product not found" });
            }

            _toastNotification.AddSuccessToastMessage("Product deleted successfully!");
            return Json(new { success = true, message = "Product deleted successfully!" });
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning(ex, "Unauthorized request when deleting product {ProductId}", id);
            return Unauthorized(new { success = false, message = "Session expired. Please login again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            _toastNotification.AddErrorToastMessage("An error occurred while deleting the product. Please try again.");
            return Json(new { success = false, message = "An error occurred while deleting the product. Please try again." });
        }
    }
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var fileContent = await _productApiService.ExportAsync();
        if (fileContent.Length == 0)
        {
             _toastNotification.AddErrorToastMessage("Export failed.");
             return RedirectToAction("Index");
        }
        return File(fileContent, "text/csv", $"products_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
             return Json(new { success = false, message = "Please select a file." });
        }

        var success = await _productApiService.ImportAsync(file);
        if (success)
        {
             _toastNotification.AddSuccessToastMessage("Products imported successfully.");
             return Json(new { success = true });
        }
        else
        {
             return Json(new { success = false, message = "Import failed. Use valid CSV format." });
        }
    }

    // GET: Admin/Products/DownloadTemplate
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        var csvHeader = "Name,Description,Price,Stock\n";
        var fileContent = System.Text.Encoding.UTF8.GetBytes(csvHeader);
        return File(fileContent, "text/csv", "product_import_template.csv");
    }
}

