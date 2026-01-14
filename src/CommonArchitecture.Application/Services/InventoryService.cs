using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Core.Models;

namespace CommonArchitecture.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<InventoryDto>> GetInventorySummaryAsync(InventoryQueryParameters parameters)
    {
        var items = await _unitOfWork.InventoryTransactions.GetInventorySummaryAsync(
            parameters.SearchTerm,
            parameters.CategoryId,
            parameters.SortBy ?? "ProductName",
            parameters.SortOrder ?? "asc",
            parameters.PageNumber,
            parameters.PageSize);

        var total = await _unitOfWork.InventoryTransactions.GetInventorySummaryCountAsync(parameters.SearchTerm, parameters.CategoryId);

        var dtos = items.Select(x => new InventoryDto
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            CategoryName = x.CategoryName,
            CurrentStock = x.CurrentStock,
            LastUpdated = x.LastUpdated
        });

        return new PaginatedResult<InventoryDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetProductTransactionsAsync(int productId)
    {
        var transactions = await _unitOfWork.InventoryTransactions.GetByProductIdAsync(productId);
        return transactions.Select(t => new InventoryTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            ProductName = t.Product?.Name ?? "N/A",
            Quantity = t.Quantity,
            TransactionType = t.TransactionType,
            Reason = t.Reason,
            ReferenceNumber = t.ReferenceNumber,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy
        });
    }

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto adjustmentDto, string? userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(adjustmentDto.ProductId);
        if (product == null) return false;

        var transaction = new InventoryTransaction
        {
            ProductId = adjustmentDto.ProductId,
            Quantity = adjustmentDto.Quantity,
            TransactionType = adjustmentDto.TransactionType,
            Reason = adjustmentDto.Reason,
            ReferenceNumber = adjustmentDto.ReferenceNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _unitOfWork.InventoryTransactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
