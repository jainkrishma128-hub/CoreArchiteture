using Microsoft.AspNetCore.Mvc;
using CommonArchitecture.Application.Services;
using CommonArchitecture.Core.DTOs;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();
        return Ok(result);
    }
}
