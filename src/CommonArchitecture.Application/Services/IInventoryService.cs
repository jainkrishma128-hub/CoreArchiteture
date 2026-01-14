using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Application.Services;

public interface IInventoryService
{
    Task<PaginatedResult<InventoryDto>> GetInventorySummaryAsync(InventoryQueryParameters parameters);
    Task<IEnumerable<InventoryTransactionDto>> GetProductTransactionsAsync(int productId);
    Task<bool> AdjustStockAsync(StockAdjustmentDto adjustmentDto, string? userId);
}
