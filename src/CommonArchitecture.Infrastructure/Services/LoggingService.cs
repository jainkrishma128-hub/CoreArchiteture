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
        
        // Set command timeout for long-running queries
        context.Database.SetCommandTimeout(120);
        
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

public async Task<(List<DailyStatDto> DailyStats, StatusDistributionDto StatusDistribution, double AvgDuration)> GetDashboardStatsSpAsync(DateTime from, DateTime to)
{
    var dailyStats = new List<DailyStatDto>();
    var statusDistribution = new StatusDistributionDto();
    double avgDuration = 0;

    using var context = await _contextFactory.CreateDbContextAsync();
    var connection = context.Database.GetDbConnection();
    bool wasOpen = connection.State == System.Data.ConnectionState.Open;
    if (!wasOpen) await connection.OpenAsync();

    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = "sp_GetDashboardStats";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.CommandTimeout = 120; // 2 minutes timeout

        var pFrom = command.CreateParameter();
        pFrom.ParameterName = "@FromDate";
        pFrom.Value = from;
        command.Parameters.Add(pFrom);

        var pTo = command.CreateParameter();
        pTo.ParameterName = "@ToDate";
        pTo.Value = to;
        command.Parameters.Add(pTo);

        using var reader = await command.ExecuteReaderAsync();

        // 1. Daily Stats
        while (await reader.ReadAsync())
        {
            dailyStats.Add(new DailyStatDto
            {
                Date = reader.GetDateTime(0),
                Count = reader.GetInt32(1),
                AverageDuration = reader.GetDouble(2)
            });
        }

        // 2. Status Distribution
        if (await reader.NextResultAsync())
        {
            if (await reader.ReadAsync())
            {
                statusDistribution = new StatusDistributionDto
                {
                    Success = reader.GetInt32(reader.GetOrdinal("Success")),
                    ClientError = reader.GetInt32(reader.GetOrdinal("ClientError")),
                    ServerError = reader.GetInt32(reader.GetOrdinal("ServerError"))
                };
            }
        }

        // 3. Overall Average
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
                avgDuration = reader.GetDouble(0);
        }
    }
    finally
    {
        if (!wasOpen) await connection.CloseAsync();
    }

    return (dailyStats, statusDistribution, avgDuration);
}

}
