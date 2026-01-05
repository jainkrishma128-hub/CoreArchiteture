using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestLoggingController : ControllerBase
{
    private readonly ILogger<TestLoggingController> _logger;

    public TestLoggingController(ILogger<TestLoggingController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify request/response logging
    /// </summary>
    [HttpGet("success")]
    public IActionResult TestSuccess()
    {
        _logger.LogInformation("Test success endpoint called");
        return Ok(new 
        { 
            success = true, 
            message = "This request will be logged to database",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Test endpoint to verify exception logging
    /// </summary>
    [HttpGet("error")]
    public IActionResult TestError()
    {
        _logger.LogInformation("Test error endpoint called - about to throw exception");
        throw new Exception("This is a test exception that will be logged to database");
    }

    /// <summary>
    /// Test endpoint with authentication to verify UserId logging
    /// </summary>
    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult TestAuthenticated()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Authenticated test endpoint called by user {UserId}", userId);
        
        return Ok(new 
        { 
            success = true, 
            message = "This authenticated request will be logged with UserId",
            userId = userId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Test endpoint for different exception types
    /// </summary>
    [HttpGet("not-found")]
    public IActionResult TestNotFound()
    {
        throw new KeyNotFoundException("This will return 404 and be logged to database");
    }

    /// <summary>
    /// Test endpoint for bad request
    /// </summary>
    [HttpGet("bad-request")]
    public IActionResult TestBadRequest()
    {
        throw new ArgumentException("This will return 400 and be logged to database");
    }

    /// <summary>
    /// Test endpoint for unauthorized
    /// </summary>
    [HttpGet("unauthorized")]
    public IActionResult TestUnauthorized()
    {
        throw new UnauthorizedAccessException("This will return 401 and be logged to database");
    }
}
