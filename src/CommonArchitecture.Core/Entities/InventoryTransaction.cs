using CommonArchitecture.Core.Enums;

namespace CommonArchitecture.Core.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } // +ve for add, -ve for remove
    public TransactionType TransactionType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; } // Order ID, PO Number etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Navigation property
    public Product? Product { get; set; }
}
