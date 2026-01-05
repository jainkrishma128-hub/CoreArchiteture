namespace CommonArchitecture.Core.DTOs;

public class DashboardStatsDto
{
    public List<DailyStatDto> UserRegistrations { get; set; } = new();
    public List<DailyStatDto> ApiCalls { get; set; } = new();
    public StatusDistributionDto StatusCodes { get; set; } = new();
    public double AverageResponseTime { get; set; }
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
