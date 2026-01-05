using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CommonArchitecture.Web.Middlewares;

public class WebExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WebExceptionLoggingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public WebExceptionLoggingMiddleware(
        RequestDelegate next, 
        ILogger<WebExceptionLoggingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in web app for {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            // Get user ID from session
            var userId = context.Session?.GetString("UserId");

            // Create error log entry
            var error = new ErrorLog
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                Path = context.Request.Path + context.Request.PathBase,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            try
            {
                var loggingService = context.RequestServices.GetService(typeof(ILoggingService)) as ILoggingService;
                if (loggingService != null)
                {
                    await loggingService.LogErrorAsync(error);
                    _logger.LogInformation("Web exception saved to database with ID: {ErrorId}", error.Id);
                }
                else
                {
                    _logger.LogWarning("ILoggingService not available from request services");
                }
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to persist web exception to DB");
            }

            // Redirect to appropriate error page based on exception type
            await HandleExceptionRedirect(context, ex);
        }
    }

    private async Task HandleExceptionRedirect(HttpContext context, Exception exception)
    {
        // Don't redirect if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot redirect to error page");
            return;
        }

        // Determine error page based on exception type
        string errorPath;
        int statusCode;

        if (exception is UnauthorizedAccessException)
        {
            errorPath = "/Error/AccessDenied";
            statusCode = 403;
        }
        else if (exception is KeyNotFoundException || exception.Message.Contains("not found"))
        {
            errorPath = "/Error/NotFound";
            statusCode = 404;
        }
        else
        {
            errorPath = "/Error/Index";
            statusCode = 500;
        }

        context.Response.StatusCode = statusCode;
        context.Response.Redirect(errorPath);
        await Task.CompletedTask;
    }
}
