using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Models;

namespace CommonArchitecture.Core.Interfaces;

public interface IInventoryTransactionRepository
{
    Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(int productId);
    Task<InventoryTransaction> AddAsync(InventoryTransaction transaction);
    Task<int> GetCurrentStockAsync(int productId);
    Task<IEnumerable<ProductInventorySummary>> GetInventorySummaryAsync(string? searchTerm, int? categoryId, string sortBy, string sortOrder, int pageNumber, int pageSize);
    Task<int> GetInventorySummaryCountAsync(string? searchTerm, int? categoryId);
}
