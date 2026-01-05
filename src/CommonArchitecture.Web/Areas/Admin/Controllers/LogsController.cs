using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Admin (1) only
public class LogsController : Controller
{
    private readonly ILogApiService _logApiService;
    private readonly ILogger<LogsController> _logger;

    public LogsController(ILogApiService logApiService, ILogger<LogsController> logger)
    {
        _logApiService = logApiService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] LogQueryParameters parameters)
    {
        try
        {
            var result = await _logApiService.GetAllAsync(parameters);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching logs");
            return Json(new { success = false, message = "Error fetching logs" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var log = await _logApiService.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            // If it's an AJAX request (modal), return partial view or JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, data = log });
            }

            return View(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching log details {LogId}", id);
            return StatusCode(500);
        }
    }
}
