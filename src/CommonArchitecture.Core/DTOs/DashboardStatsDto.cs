namespace CommonArchitecture.Core.DTOs;

public class DashboardStatsDto
{
    public List<DailyStatDto> UserRegistrations { get; set; } = new();
    
    // Ecommerce Metrics
    public decimal TotalRevenue { get; set; }
    public int RecentOrdersCount { get; set; }
    public int LowStockCount { get; set; }
    public List<DailyRevenueDto> RevenueTrends { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<TopProductDto> TopSellingProducts { get; set; } = new();
}


public class RecentOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal Revenue { get; set; }
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}


public class DailyStatDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public double AverageDuration { get; set; } // For API calls
}

public class StatusDistributionDto
{
    public int Success { get; set; }    // 2xx
    public int ClientError { get; set; } // 4xx
    public int ServerError { get; set; } // 5xx
}

