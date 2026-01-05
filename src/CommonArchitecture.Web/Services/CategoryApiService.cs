using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace CommonArchitecture.Web.Services;

public class CategoryApiService : ICategoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoryApiService> _logger;

    public CategoryApiService(HttpClient httpClient, ILogger<CategoryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResult<CategoryDto>> GetAllAsync(CategoryQueryParameters parameters)
    {
        try
        {
            // Build query string
            var queryString = $"api/categories?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}&SortBy={parameters.SortBy}&SortOrder={parameters.SortOrder}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            }

            var result = await _httpClient.GetFromJsonAsync<PaginatedResult<CategoryDto>>(queryString);
            return result ?? new PaginatedResult<CategoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories from API");
            return new PaginatedResult<CategoryDto>();
        }
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CategoryDto>($"api/categories/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category {CategoryId} from API", id);
            return null;
        }
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/categories", createDto);
            response.EnsureSuccessStatusCode();
            
            var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
            return category ?? throw new Exception("Failed to deserialize created category");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category via API");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto updateDto)
    {
        try
        {
            _logger.LogInformation("Attempting to update category {CategoryId} with data: {@UpdateData}", id, updateDto);
            _logger.LogInformation("Full URL: api/categories/{CategoryId}", id);
    
            var response = await _httpClient.PutAsJsonAsync($"api/categories/{id}", updateDto);
     
            _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Category update failed. Status: {StatusCode}, Error: {ErrorContent}", 
                    response.StatusCode, errorContent);
                
                // Log 401 Unauthorized separately for JWT issues
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("JWT token issue - Unauthorized (401). Check token expiration.");
                }
          
                // Log 404 separately
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("Category {CategoryId} not found in API database", id);
                }

                return false;
            }

            _logger.LogInformation("Category {CategoryId} updated successfully", id);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException updating category {CategoryId}. Status: {StatusCode}, Message: {Message}", 
                id, ex.StatusCode, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId} via API. Exception Type: {ExceptionType}, Message: {ExceptionMessage}", 
                id, ex.GetType().Name, ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete category {CategoryId}", id);
  
            var response = await _httpClient.DeleteAsync($"api/categories/{id}");
      
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Category delete failed. Status: {StatusCode}, Error: {ErrorContent}", 
                    response.StatusCode, errorContent);
         
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("JWT token issue - Unauthorized (401). Check token expiration.");
                }
      
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("Category {CategoryId} not found in API", id);
                }
          
                return false;
            }

            _logger.LogInformation("Category {CategoryId} deleted successfully", id);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException deleting category {CategoryId}. Message: {Message}", id, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId} via API. Exception: {ExceptionMessage}", id, ex.Message);
            return false;
        }
    }

    public async Task<byte[]> ExportAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/categories/export");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting categories");
            return Array.Empty<byte>();
        }
    }

    public async Task<bool> ImportAsync(IFormFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("api/categories/import", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Import failed: {Error}", error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing categories");
            return false;
        }
    }
}