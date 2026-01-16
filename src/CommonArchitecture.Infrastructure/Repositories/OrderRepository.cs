using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommonArchitecture.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<string> GetLastOrderNumberAsync()
    {
        var lastOrder = await _context.Orders
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();
        return lastOrder?.OrderNumber ?? string.Empty;
    }

    public async Task<(decimal TotalRevenue, int TotalOrdersCount)> GetDashboardSummaryAsync(DateTime from, DateTime to)
    {
        var query = _context.Orders.Where(o => o.OrderDate >= from && o.OrderDate <= to && o.Status != Core.Enums.OrderStatus.Cancelled);
        
        var totalRevenue = await query
            .Where(o => o.Status == Core.Enums.OrderStatus.Delivered)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            
        var totalOrdersCount = await query.CountAsync();
        
        return (totalRevenue, totalOrdersCount);
    }

    public async Task<List<Order>> GetRecentOrdersAsync(int count)
    {
        return await _context.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<(DateTime Date, decimal Amount, int Count)>> GetRevenueTrendsAsync(DateTime from, DateTime to)
    {
        var trends = await _context.Orders
            .Where(o => o.OrderDate >= from && o.OrderDate <= to && o.Status != Core.Enums.OrderStatus.Cancelled)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(o => o.TotalAmount), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return trends.Select(x => (x.Date, x.Amount, x.Count)).ToList();
    }


    public async Task<List<(int ProductId, string ProductName, int TotalSold, decimal Revenue)>> GetTopSellingProductsAsync(int count)
    {
        var topProducts = await _context.Orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                TotalSold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .ToListAsync();

        return topProducts.Select(p => (p.ProductId, p.ProductName, p.TotalSold, p.Revenue)).ToList();
    }
}


