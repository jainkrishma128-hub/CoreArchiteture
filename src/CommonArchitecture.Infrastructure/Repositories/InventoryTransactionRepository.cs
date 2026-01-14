using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Core.Models;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CommonArchitecture.Infrastructure.Repositories;

public class InventoryTransactionRepository : IInventoryTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(int productId)
    {
        return await _context.InventoryTransactions
            .Where(it => it.ProductId == productId)
            .OrderByDescending(it => it.CreatedAt)
            .ToListAsync();
    }

    public async Task<InventoryTransaction> AddAsync(InventoryTransaction transaction)
    {
        await _context.InventoryTransactions.AddAsync(transaction);
        return transaction;
    }

    public async Task<int> GetCurrentStockAsync(int productId)
    {
        return await _context.InventoryTransactions
            .Where(it => it.ProductId == productId)
            .SumAsync(it => it.Quantity);
    }

    public async Task<IEnumerable<ProductInventorySummary>> GetInventorySummaryAsync(string? searchTerm, int? categoryId, string sortBy, string sortOrder, int pageNumber, int pageSize)
    {
        var query = _context.Products
            .GroupJoin(_context.InventoryTransactions,
                p => p.Id,
                it => it.ProductId,
                (p, transactions) => new ProductInventorySummary
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    CategoryId = p.CategoryId,
                    CurrentStock = transactions.Sum(it => (int?)it.Quantity) ?? 0,
                    LastUpdated = transactions.Max(it => (DateTime?)it.CreatedAt)
                });

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.ProductName.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        // Apply sorting
        if (string.IsNullOrWhiteSpace(sortBy)) sortBy = "ProductName";
        var isDescending = sortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;
        query = query.OrderBy($"{sortBy} {(isDescending ? "descending" : "ascending")}");

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetInventorySummaryCountAsync(string? searchTerm, int? categoryId)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Name.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        return await query.CountAsync();
    }
}
