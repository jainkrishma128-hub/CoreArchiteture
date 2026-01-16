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

        // UoW operations must be sequential because they share a single DbContext
        var registrations = await _unitOfWork.Users.GetDailyRegistrationsAsync(from, to).ConfigureAwait(false);
        var (totalRevenue, recentOrdersCount) = await _unitOfWork.Orders.GetDashboardSummaryAsync(from, to).ConfigureAwait(false);
        var revenueTrends = await _unitOfWork.Orders.GetRevenueTrendsAsync(from, to).ConfigureAwait(false);
        var recentOrdersList = await _unitOfWork.Orders.GetRecentOrdersAsync(5).ConfigureAwait(false);
        var topProductsRaw = await _unitOfWork.Orders.GetTopSellingProductsAsync(5).ConfigureAwait(false);
        var inventorySummary = await _unitOfWork.InventoryTransactions.GetInventorySummaryAsync(null, null, "CurrentStock", "asc", 1, 1000).ConfigureAwait(false);

        var lowStockCount = inventorySummary.Count(i => i.CurrentStock < 10);

        var result = new DashboardStatsDto
        {
            UserRegistrations = registrations,
            
            // Ecommerce Data
            TotalRevenue = totalRevenue,
            RecentOrdersCount = recentOrdersCount,
            LowStockCount = lowStockCount,
            RevenueTrends = revenueTrends.Select(t => new DailyRevenueDto { Date = t.Date, Amount = t.Amount, OrderCount = t.Count }).ToList(),
            RecentOrders = recentOrdersList.Select(o => new RecentOrderDto

            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.CustomerName,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                OrderDate = o.OrderDate
            }).ToList(),
            TopSellingProducts = topProductsRaw.Select(p => new TopProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                TotalSold = p.TotalSold,
                Revenue = p.Revenue
            }).ToList()
        };



        // OPTIMIZATION: Cache the result for 5 minutes
        _memoryCache.Set(DashboardStatsCacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromMinutes(2)
        });

        return result;
    }
}
