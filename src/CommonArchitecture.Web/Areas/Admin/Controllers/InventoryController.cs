using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1, 2)] // Admin and Product Manager
public class InventoryController : Controller
{
    private readonly IInventoryApiService _inventoryApiService;
    private readonly ICategoryApiService _categoryApiService;
    private readonly ILogger<InventoryController> _logger;
    private readonly IToastNotification _toastNotification;

    public InventoryController(
        IInventoryApiService inventoryApiService,
        ICategoryApiService categoryApiService,
        ILogger<InventoryController> logger,
        IToastNotification toastNotification)
    {
        _inventoryApiService = inventoryApiService;
        _categoryApiService = categoryApiService;
        _logger = logger;
        _toastNotification = toastNotification;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] InventoryQueryParameters parameters)
    {
        try
        {
            var result = await _inventoryApiService.GetSummaryAsync(parameters);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching inventory");
            return Json(new { success = false, message = "Error fetching inventory" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(int productId)
    {
        try
        {
            var result = await _inventoryApiService.GetTransactionsAsync(productId);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions");
            return Json(new { success = false, message = "Error fetching transactions" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust([FromBody] StockAdjustmentDto adjustmentDto)
    {
        try
        {
            var success = await _inventoryApiService.AdjustStockAsync(adjustmentDto);
            if (success)
            {
                _toastNotification.AddSuccessToastMessage("Stock adjusted successfully!");
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to adjust stock" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock");
            return Json(new { success = false, message = "An error occurred" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var result = await _categoryApiService.GetAllAsync(new CategoryQueryParameters { PageSize = 100 });
            return Json(new { success = true, data = result.Items });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            return Json(new { success = false, message = "Error loading categories" });
        }
    }
}
