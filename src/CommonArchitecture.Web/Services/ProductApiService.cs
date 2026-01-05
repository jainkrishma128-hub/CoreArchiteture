using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace CommonArchitecture.Web.Services;

public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(HttpClient httpClient, ILogger<ProductApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResult<ProductDto>> GetAllAsync(ProductQueryParameters parameters)
    {
        try
        {
            // Build query string
            var queryString = $"api/products?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}&SortBy={parameters.SortBy}&SortOrder={parameters.SortOrder}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            }

            var result = await _httpClient.GetFromJsonAsync<PaginatedResult<ProductDto>>(queryString);
            return result ?? new PaginatedResult<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from API");
            return new PaginatedResult<ProductDto>();
        }
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId} from API", id);
            return null;
        }
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", createDto);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            return product ?? throw new Exception("Failed to deserialize created product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product via API");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto updateDto)
    {
        try
        {
            _logger.LogInformation("Attempting to update product {ProductId} with data: {@UpdateData}", id, updateDto);
            _logger.LogInformation("Full URL: api/products/{ProductId}", id);
    
            var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", updateDto);
     
            _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
                 {
              var errorContent = await response.Content.ReadAsStringAsync();
                 _logger.LogError("Product update failed. Status: {StatusCode}, Error: {ErrorContent}", 
                 response.StatusCode, errorContent);
                
               // Log 401 Unauthorized separately for JWT issues
                  if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                   {
                   _logger.LogError("JWT token issue - Unauthorized (401). Check token expiration.");
              }
          
              // Log 404 separately
              if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
  _logger.LogError("Product {ProductId} not found in API database", id);
  }

                    return false;
       }

 _logger.LogInformation("Product {ProductId} updated successfully", id);
            return true;
    }
 catch (HttpRequestException ex)
    {
            _logger.LogError(ex, "HttpRequestException updating product {ProductId}. Status: {StatusCode}, Message: {Message}", 
    id, ex.StatusCode, ex.Message);
     return false;
   }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Error updating product {ProductId} via API. Exception Type: {ExceptionType}, Message: {ExceptionMessage}", 
     id, ex.GetType().Name, ex.Message);
     return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
  _logger.LogInformation("Attempting to delete product {ProductId}", id);
  
  var response = await _httpClient.DeleteAsync($"api/products/{id}");
      
   if (!response.IsSuccessStatusCode)
   {
            var errorContent = await response.Content.ReadAsStringAsync();
      _logger.LogError("Product delete failed. Status: {StatusCode}, Error: {ErrorContent}", 
      response.StatusCode, errorContent);
         
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
  {
         _logger.LogError("JWT token issue - Unauthorized (401). Check token expiration.");
      }
      
         if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
 {
          _logger.LogError("Product {ProductId} not found in API", id);
     }
          
         return false;
    }

       _logger.LogInformation("Product {ProductId} deleted successfully", id);
          return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException deleting product {ProductId}. Message: {Message}", id, ex.Message);
     return false;
        }
   catch (Exception ex)
  {
 _logger.LogError(ex, "Error deleting product {ProductId} via API. Exception: {ExceptionMessage}", id, ex.Message);
         return false;
        }
    }
    public async Task<byte[]> ExportAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/products/export");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting products");
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

            var response = await _httpClient.PostAsync("api/products/import", content);
            
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
            _logger.LogError(ex, "Error importing products");
            return false;
        }
    }
}
