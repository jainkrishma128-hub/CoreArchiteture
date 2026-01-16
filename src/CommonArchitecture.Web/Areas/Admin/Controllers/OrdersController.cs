using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Enums;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Admin only for Order Management
public class OrdersController : Controller
{
    private readonly IOrderApiService _orderApiService;
    private readonly ILogger<OrdersController> _logger;
    private readonly IToastNotification _toastNotification;

    public OrdersController(
        IOrderApiService orderApiService,
        ILogger<OrdersController> logger,
        IToastNotification toastNotification)
    {
        _orderApiService = orderApiService;
        _logger = logger;
        _toastNotification = toastNotification;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _orderApiService.GetAllOrdersAsync();
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return Json(new { success = false, message = "Error fetching orders" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var order = await _orderApiService.GetOrderByIdAsync(id);
            if (order == null)
            {
                _toastNotification.AddErrorToastMessage("Order not found");
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order details {OrderId}", id);
            _toastNotification.AddErrorToastMessage("Error fetching order details");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusDto updateDto)
    {
        try
        {
            var success = await _orderApiService.UpdateStatusAsync(updateDto.OrderId, updateDto);
            if (success)
            {
                _toastNotification.AddSuccessToastMessage("Order status updated successfully!");
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to update order status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", updateDto.OrderId);
            return Json(new { success = false, message = "An error occurred" });
        }
    }
}
