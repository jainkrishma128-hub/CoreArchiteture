using System.Net;
using System.Security.Claims;
using System.Text.Json;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;

        // Handle different exception types and set specific status codes
        if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized;
        }
        else if (exception is ArgumentException || exception is ArgumentNullException)
        {
            code = HttpStatusCode.BadRequest;
        }
        else if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
        }

        // Get user ID from JWT claims
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Create error log entry
        var errorLog = new ErrorLog
        {
            Message = exception.Message,
            StackTrace = exception.StackTrace,
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
                await loggingService.LogErrorAsync(errorLog);
                _logger.LogInformation("Exception saved to database with ID: {ErrorId}", errorLog.Id);
            }
            else
            {
                _logger.LogWarning("ILoggingService not available - exception not saved to database");
            }
        }
        catch (Exception logEx)
        {
            // Don't let logging errors break the exception handling
            _logger.LogError(logEx, "Failed to save exception to database");
        }

        // Return error response to client
        var problem = new
        {
            success = false,
            message = code == HttpStatusCode.InternalServerError 
                ? "An unexpected error occurred. Please try again later." 
                : exception.Message,
            detail = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
                ? exception.StackTrace 
                : null // Only show stack trace in development
        };

        var result = JsonSerializer.Serialize(problem);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        await context.Response.WriteAsync(result);
    }
}
