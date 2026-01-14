using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;

namespace CommonArchitecture.Web.Services;

public class InventoryApiService : IInventoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiService> _logger;

    public InventoryApiService(HttpClient httpClient, ILogger<InventoryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResult<InventoryDto>> GetSummaryAsync(InventoryQueryParameters parameters)
    {
        try
        {
            var queryString = $"api/inventory?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}";
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm)) queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            if (parameters.CategoryId.HasValue) queryString += $"&CategoryId={parameters.CategoryId}";
            if (!string.IsNullOrWhiteSpace(parameters.SortBy)) queryString += $"&SortBy={parameters.SortBy}";
            if (!string.IsNullOrWhiteSpace(parameters.SortOrder)) queryString += $"&SortOrder={parameters.SortOrder}";

            return await _httpClient.GetFromJsonAsync<PaginatedResult<InventoryDto>>(queryString) ?? new PaginatedResult<InventoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching inventory summary");
            return new PaginatedResult<InventoryDto>();
        }
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int productId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<InventoryTransactionDto>>($"api/inventory/{productId}/transactions") ?? Enumerable.Empty<InventoryTransactionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching inventory transactions for product {ProductId}", productId);
            return Enumerable.Empty<InventoryTransactionDto>();
        }
    }

    public async Task<bool> AdjustStockAsync(StockAdjustmentDto adjustmentDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/inventory/adjust", adjustmentDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock");
            return false;
        }
    }
}
