using CommonArchitecture.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AuthorizeRole(1)] // Only Admin can access
public class HangfireJobsController : Controller
{
    private readonly ILogger<HangfireJobsController> _logger;

    public HangfireJobsController(ILogger<HangfireJobsController> logger)
    {
        _logger = logger;
    }

    // GET: Admin/HangfireJobs
    public IActionResult Index()
    {
        ViewData["Title"] = "Hangfire Jobs";
        return View();
    }
}

