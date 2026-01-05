using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // Only Admin can access logs
public class LogsController : ControllerBase
{
    private readonly ILoggingService _loggingService;

    public LogsController(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<LogDto>>> GetAll([FromQuery] LogQueryParameters parameters)
    {
        var (logs, totalCount) = await _loggingService.GetLogsAsync(
            parameters.PageNumber,
            parameters.PageSize,
            parameters.SearchTerm,
            parameters.SortBy,
            parameters.SortOrder,
            parameters.FromDate,
            parameters.ToDate,
            parameters.StatusCode,
            parameters.Method);

        var logDtos = logs.Select(l => new LogDto
        {
            Id = l.Id,
            Method = l.Method,
            Path = l.Path,
            QueryString = l.QueryString,
            // Optimization: Exclude large JSON payloads from the list view to reduce latency/bandwidth.
            // These are still available via the GetById endpoint for the details modal.
            RequestBody = null, 
            ResponseStatusCode = l.ResponseStatusCode,
            ResponseBody = null,
            DurationMs = l.DurationMs,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            UserId = l.UserId,
            CreatedAt = l.CreatedAt
        }).ToList();

        return Ok(new PaginatedResult<LogDto>
        {
            Items = logDtos,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LogDto>> GetById(int id)
    {
        var log = await _loggingService.GetLogByIdAsync(id);
        if (log == null)
            return NotFound();

        return Ok(new LogDto
        {
            Id = log.Id,
            Method = log.Method,
            Path = log.Path,
            QueryString = log.QueryString,
            RequestBody = log.RequestBody,
            ResponseStatusCode = log.ResponseStatusCode,
            ResponseBody = log.ResponseBody,
            DurationMs = log.DurationMs,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            UserId = log.UserId,
            CreatedAt = log.CreatedAt
        });
    }
}
