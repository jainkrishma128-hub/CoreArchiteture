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
        // Enforce max page size
        if (parameters.PageSize > 100) 
            parameters.PageSize = 100;
        if (parameters.PageSize < 1) 
            parameters.PageSize = 10;

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

        return Ok(new PaginatedResult<LogDto>
        {
            Items = logs.Select(log => new LogDto
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
            }).ToList(),
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
