using CommonArchitecture.Core.DTOs;

namespace CommonArchitecture.Web.Services;

public class DashboardApiService : IDashboardApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardApiService> _logger;

    public DashboardApiService(HttpClient httpClient, ILogger<DashboardApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DashboardStatsDto?> GetStatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/dashboard/stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard stats");
            return null;
        }
    }
}
