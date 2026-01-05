using CommonArchitecture.Web.Filters;
using CommonArchitecture.Web.Services;
using CommonArchitecture.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeUser]
[HasPermission("Dashboard", PermissionType.View)]
public class DashboardController : Controller
{
    private readonly IDashboardApiService _dashboardService;

    public DashboardController(IDashboardApiService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        if (stats == null) return Json(new { success = false });
        return Json(new { success = true, data = stats });
    }
}
