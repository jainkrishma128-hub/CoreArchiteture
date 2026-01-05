using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using CommonArchitecture.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CommonArchitecture.Infrastructure.Services;

public class LoggingService : ILoggingService
{
 private readonly ApplicationDbContext _db;

 public LoggingService(ApplicationDbContext db)
 {
 _db = db;
 }

 public async Task LogErrorAsync(ErrorLog error)
 {
 _db.ErrorLogs.Add(error);
 await _db.SaveChangesAsync();
 }

    public async Task LogRequestResponseAsync(RequestResponseLog log)
    {
        _db.RequestResponseLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<(IEnumerable<RequestResponseLog> Logs, int TotalCount)> GetLogsAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null, 
        string? sortBy = null, 
        string sortOrder = "desc",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? statusCode = null,
        string? method = null)
    {
        var query = _db.RequestResponseLogs.AsNoTracking().AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(l => 
                l.Path.Contains(searchTerm) || 
                (l.RequestBody != null && l.RequestBody.Contains(searchTerm)) ||
                (l.ResponseBody != null && l.ResponseBody.Contains(searchTerm)) ||
                (l.IpAddress != null && l.IpAddress.Contains(searchTerm)));
        }

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        if (statusCode.HasValue)
            query = query.Where(l => l.ResponseStatusCode == statusCode.Value);

        if (!string.IsNullOrWhiteSpace(method))
            query = query.Where(l => l.Method == method);

        // Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var isDesc = sortOrder?.ToLower() == "desc";
            query = sortBy.ToLower() switch
            {
                "createdat" => isDesc ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
                "durationms" => isDesc ? query.OrderByDescending(l => l.DurationMs) : query.OrderBy(l => l.DurationMs),
                "responsestatuscode" => isDesc ? query.OrderByDescending(l => l.ResponseStatusCode) : query.OrderBy(l => l.ResponseStatusCode),
                "method" => isDesc ? query.OrderByDescending(l => l.Method) : query.OrderBy(l => l.Method),
                "path" => isDesc ? query.OrderByDescending(l => l.Path) : query.OrderBy(l => l.Path),
                _ => query.OrderByDescending(l => l.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(l => l.CreatedAt);
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        
        var logs = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            query.Skip((pageNumber - 1) * pageSize).Take(pageSize));

        return (logs, totalCount);
    }

    public async Task<RequestResponseLog?> GetLogByIdAsync(int id)
    {
        return await _db.RequestResponseLogs.FindAsync(id);
    }

    public async Task<(List<DailyStatDto> DailyStats, StatusDistributionDto StatusDistribution, double AvgDuration)> GetDashboardStatsAsync(DateTime from, DateTime to)
    {
        var logs = _db.RequestResponseLogs
            .Where(l => l.CreatedAt >= from && l.CreatedAt <= to);

        // Group by Date for API Calls
        var dailyStats = await logs
            .GroupBy(l => l.CreatedAt.Date)
            .Select(g => new DailyStatDto
            {
                Date = g.Key,
                Count = g.Count(),
                AverageDuration = g.Average(l => l.DurationMs)
            })
            .OrderBy(s => s.Date)
            .ToListAsync();

        var statusDist = await logs
            .GroupBy(l => 1)
            .Select(g => new StatusDistributionDto
            {
                Success = g.Count(l => l.ResponseStatusCode >= 200 && l.ResponseStatusCode < 300),
                ClientError = g.Count(l => l.ResponseStatusCode >= 400 && l.ResponseStatusCode < 500),
                ServerError = g.Count(l => l.ResponseStatusCode >= 500)
            })
            .FirstOrDefaultAsync() ?? new StatusDistributionDto();

        var avgDuration = await logs.AnyAsync() 
            ? await logs.AverageAsync(l => l.DurationMs) 
            : 0;

        return (dailyStats, statusDist, avgDuration);
    }
}
