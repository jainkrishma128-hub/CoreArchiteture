using CommonArchitecture.Core.DTOs;
using CommonArchitecture.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CommonArchitecture.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;
    private readonly IMemoryCache _memoryCache;
    private const string DashboardStatsCacheKey = "DashboardStats_30Days";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5); // Cache for 5 minutes

    public DashboardService(IUnitOfWork unitOfWork, ILoggingService loggingService, IMemoryCache memoryCache)
    {
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
        _memoryCache = memoryCache;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        // OPTIMIZATION: Check cache first to avoid database queries
        if (_memoryCache.TryGetValue(DashboardStatsCacheKey, out DashboardStatsDto? cachedStats) && cachedStats != null)
        {
            return cachedStats;
        }

        var to = DateTime.UtcNow;
        var from = to.AddDays(-30);

        // Optimization: Execute queries in parallel
        // LoggingService uses DbContextFactory (new context), so it's safe to run alongside UnitOfWork
        var registrationsTask = _unitOfWork.Users.GetDailyRegistrationsAsync(from, to);
        var logsTask = _loggingService.GetDashboardStatsAsync(from, to);

        await Task.WhenAll(registrationsTask, logsTask).ConfigureAwait(false);

        var registrations = await registrationsTask;
        var (apiCalls, statusDist, avgDuration) = await logsTask;

        var result = new DashboardStatsDto
        {
            UserRegistrations = registrations,
            ApiCalls = apiCalls,
            StatusCodes = statusDist,
            AverageResponseTime = avgDuration
        };

        // OPTIMIZATION: Cache the result for 5 minutes
        _memoryCache.Set(DashboardStatsCacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromMinutes(2) // Reset if accessed within 2 minutes
        });

        return result;
    }
}
