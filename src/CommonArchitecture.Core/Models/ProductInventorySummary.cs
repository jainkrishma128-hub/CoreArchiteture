using System;

namespace CommonArchitecture.Core.Models;

public class ProductInventorySummary
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int CurrentStock { get; set; }
    public DateTime? LastUpdated { get; set; }
}
