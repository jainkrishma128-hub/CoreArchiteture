using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IInventoryApiService
{
    Task<PaginatedResult<InventoryDto>> GetSummaryAsync(InventoryQueryParameters parameters);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int productId);
    Task<bool> AdjustStockAsync(StockAdjustmentDto adjustmentDto);
}
