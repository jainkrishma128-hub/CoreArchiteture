using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace CommonArchitecture.API.Middlewares;

public class RequestResponseLoggingMiddleware
{
 private readonly RequestDelegate _next;
 private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

 public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
 {
 _next = next;
 _logger = logger;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 var start = DateTime.UtcNow;

 // Read request
 context.Request.EnableBuffering();
 string? requestBody = null;
 if (context.Request.ContentLength >0)
 {
 context.Request.Body.Position =0;
 using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
 requestBody = await reader.ReadToEndAsync();
 context.Request.Body.Position =0;
 }

 // Capture response
 var originalBodyStream = context.Response.Body;
 await using var responseBody = new MemoryStream();
 context.Response.Body = responseBody;

 try
 {
 await _next(context);
 }
 finally
 {
 context.Response.Body.Seek(0, SeekOrigin.Begin);
 string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
 context.Response.Body.Seek(0, SeekOrigin.Begin);

 var duration = (long)(DateTime.UtcNow - start).TotalMilliseconds;

 // Get user ID from JWT claims
 var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

 var log = new RequestResponseLog
 {
 Method = context.Request.Method,
 Path = context.Request.Path + context.Request.PathBase,
 QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
 RequestBody = Truncate(requestBody),
 ResponseBody = Truncate(responseText),
 ResponseStatusCode = context.Response.StatusCode,
 DurationMs = duration,
 IpAddress = context.Connection.RemoteIpAddress?.ToString(),
 UserAgent = context.Request.Headers["User-Agent"].ToString(),
 UserId = userId,
 CreatedAt = DateTime.UtcNow
 };

 try
 {
 // Resolve scoped logging service from the request's IServiceProvider
 var loggingService = context.RequestServices.GetService(typeof(ILoggingService)) as ILoggingService;
 if (loggingService != null)
 {
 await loggingService.LogRequestResponseAsync(log);
 }
 else
 {
 _logger.LogWarning("ILoggingService not available from request services");
 }
 }
 catch (Exception ex)
 {
 // Logging must not break request pipeline
 _logger.LogError(ex, "Failed to save request/response log");
 }

 // copy the contents of the new memory stream (which contains the response) to the original stream.
 await responseBody.CopyToAsync(originalBodyStream);
 context.Response.Body = originalBodyStream;
 }
 }

 private static string? Truncate(string? value, int max =8000) =>
 string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value.Substring(0, max));
}
