using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public class LogApiService : ILogApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LogApiService> _logger;

    public LogApiService(HttpClient httpClient, ILogger<LogApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResult<LogDto>> GetAllAsync(LogQueryParameters parameters)
    {
        try
        {
            var queryString = $"api/logs?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}&SortOrder={parameters.SortOrder}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
                
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&SortBy={Uri.EscapeDataString(parameters.SortBy)}";
                
            if (parameters.FromDate.HasValue)
                queryString += $"&FromDate={parameters.FromDate.Value:O}";
                
            if (parameters.ToDate.HasValue)
                queryString += $"&ToDate={parameters.ToDate.Value:O}";
                
            if (parameters.StatusCode.HasValue)
                queryString += $"&StatusCode={parameters.StatusCode}";
                
            if (!string.IsNullOrWhiteSpace(parameters.Method))
                queryString += $"&Method={parameters.Method}";

            var result = await _httpClient.GetFromJsonAsync<PaginatedResult<LogDto>>(queryString);
            return result ?? new PaginatedResult<LogDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching logs from API");
            return new PaginatedResult<LogDto>();
        }
    }

    public async Task<LogDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<LogDto>($"api/logs/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching log {LogId} from API", id);
            return null;
        }
    }
}
