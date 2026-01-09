using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using CommonArchitecture.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CommonArchitecture.Infrastructure.Services;

public class LoggingService : ILoggingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public LoggingService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task LogErrorAsync(ErrorLog error)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.ErrorLogs.Add(error);
        await context.SaveChangesAsync();
    }

    public async Task LogRequestResponseAsync(RequestResponseLog log)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.RequestResponseLogs.Add(log);
        await context.SaveChangesAsync();
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
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.RequestResponseLogs.AsNoTracking().AsQueryable();

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

        var totalCount = await query.CountAsync();
        var logs = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (logs, totalCount);
    }

    public async Task<RequestResponseLog?> GetLogByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RequestResponseLogs.FindAsync(id);
    }

    public async Task<(List<DailyStatDto> DailyStats, StatusDistributionDto StatusDistribution, double AvgDuration)> GetDashboardStatsAsync(DateTime from, DateTime to)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Optimization: Perform aggregation in database instead of fetching all records into memory
        
        // 1. Daily Stats (Group by Date)
        var dailyStats = await context.RequestResponseLogs
            .AsNoTracking()
            .Where(l => l.CreatedAt >= from && l.CreatedAt <= to)
            .GroupBy(l => l.CreatedAt.Date)
            .Select(g => new DailyStatDto
            {
                Date = g.Key,
                Count = g.Count(),
                AverageDuration = g.Average(l => (double)l.DurationMs)
            })
            .OrderBy(s => s.Date)
            .ToListAsync();

        // 2. Status Distribution & Overall Average
        var stats = await context.RequestResponseLogs
            .AsNoTracking()
            .Where(l => l.CreatedAt >= from && l.CreatedAt <= to)
            .GroupBy(x => 1) // Group by constant to aggregate the whole filtered set
            .Select(g => new
            {
                Success = g.Count(l => l.ResponseStatusCode >= 200 && l.ResponseStatusCode < 300),
                ClientError = g.Count(l => l.ResponseStatusCode >= 400 && l.ResponseStatusCode < 500),
                ServerError = g.Count(l => l.ResponseStatusCode >= 500),
                AvgDuration = g.Average(l => (double)l.DurationMs)
            })
            .FirstOrDefaultAsync();

        var statusDist = new StatusDistributionDto
        {
            Success = stats?.Success ?? 0,
            ClientError = stats?.ClientError ?? 0,
            ServerError = stats?.ServerError ?? 0
        };

        var avgDuration = stats?.AvgDuration ?? 0;

        return (dailyStats, statusDist, avgDuration);
    }
}
