using CommonArchitecture.Core.Enums;

namespace CommonArchitecture.Application.DTOs;

public class InventoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class InventoryTransactionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class StockAdjustmentDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}

public class InventoryQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; } = "ProductName";
    public string? SortOrder { get; set; } = "asc";
}
