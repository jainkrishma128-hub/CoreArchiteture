using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CommonArchitecture.Web.Models;

namespace CommonArchitecture.Web.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// General error page (500 Internal Server Error)
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/Error")]
    [Route("/Error/Index")]
    public IActionResult Index()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogWarning("Error page displayed for request {RequestId}", requestId);
        
        return View(new ErrorViewModel 
        { 
            RequestId = requestId,
            ErrorMessage = "An unexpected error occurred. Please try again later.",
            StatusCode = 500
        });
    }

    /// <summary>
    /// Not Found error page (404)
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/Error/NotFound")]
    [Route("/Error/404")]
    public IActionResult NotFound()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogWarning("404 Not Found page displayed for request {RequestId}", requestId);
        
        return View(new ErrorViewModel 
        { 
            RequestId = requestId,
            ErrorMessage = "The page you are looking for could not be found.",
            StatusCode = 404
        });
    }

    /// <summary>
    /// Access Denied error page (403)
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/Error/AccessDenied")]
    [Route("/Error/403")]
    public IActionResult AccessDenied()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogWarning("403 Access Denied page displayed for request {RequestId}", requestId);
        
        return View(new ErrorViewModel 
        { 
            RequestId = requestId,
            ErrorMessage = "You do not have permission to access this resource.",
            StatusCode = 403
        });
    }

    /// <summary>
    /// Handle status code errors (fallback)
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/Error/{statusCode}")]
    public IActionResult HandleStatusCode(int statusCode)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogWarning("Status code {StatusCode} error page displayed for request {RequestId}", 
            statusCode, requestId);

        string errorMessage = statusCode switch
        {
            400 => "Bad Request. The request could not be understood by the server.",
            401 => "Unauthorized. Please log in to access this resource.",
            403 => "Forbidden. You do not have permission to access this resource.",
            404 => "Not Found. The page you are looking for could not be found.",
            500 => "Internal Server Error. An unexpected error occurred.",
            503 => "Service Unavailable. The server is temporarily unable to handle the request.",
            _ => $"An error occurred (Status Code: {statusCode})."
        };

        if (statusCode == 404)
        {
            return View("NotFound", new ErrorViewModel 
            { 
                RequestId = requestId,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            });
        }

        if (statusCode == 403)
        {
            return View("AccessDenied", new ErrorViewModel 
            { 
                RequestId = requestId,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            });
        }

        return View("Index", new ErrorViewModel 
        { 
            RequestId = requestId,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        });
    }
}
