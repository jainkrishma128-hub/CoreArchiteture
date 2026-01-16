using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<string> GetLastOrderNumberAsync();
    
    // Optimized stats methods
    Task<(decimal TotalRevenue, int TotalOrdersCount)> GetDashboardSummaryAsync(DateTime from, DateTime to);
    Task<List<Order>> GetRecentOrdersAsync(int count);
    Task<List<(DateTime Date, decimal Amount, int Count)>> GetRevenueTrendsAsync(DateTime from, DateTime to);
    Task<List<(int ProductId, string ProductName, int TotalSold, decimal Revenue)>> GetTopSellingProductsAsync(int count);

}


