using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;

namespace CommonArchitecture.Web.Services;

public class RoleApiService : IRoleApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoleApiService> _logger;

    public RoleApiService(HttpClient httpClient, ILogger<RoleApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResult<RoleDto>> GetAllAsync(RoleQueryParameters parameters)
    {
        try
        {
            // Build query string
            var queryString = $"api/roles?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}&SortBy={parameters.SortBy}&SortOrder={parameters.SortOrder}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            }

            var result = await _httpClient.GetFromJsonAsync<PaginatedResult<RoleDto>>(queryString);
            return result ?? new PaginatedResult<RoleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles from API");
            return new PaginatedResult<RoleDto>();
        }
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/roles/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RoleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role {RoleId} via API", id);
            return null;
        }
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/roles", createDto);
            response.EnsureSuccessStatusCode();
            
            var role = await response.Content.ReadFromJsonAsync<RoleDto>();
            return role ?? throw new Exception("Failed to deserialize created role");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role via API");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateRoleDto updateDto)
    {
        try
        {
            _logger.LogInformation("Attempting to update role {RoleId} with data: {@UpdateData}", id, updateDto);
            
            var response = await _httpClient.PutAsJsonAsync($"api/roles/{id}", updateDto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Role update failed. Status: {StatusCode}, Error: {ErrorContent}", 
                     response.StatusCode, errorContent);

                // Log 401 Unauthorized separately for JWT issues
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("JWT token issue - Unauthorized (401). Check token expiration.");
                }

                return false;
            }

            _logger.LogInformation("Role {RoleId} updated successfully", id);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException updating role {RoleId}. Message: {Message}", id, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId} via API", id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete role {RoleId}", id);
            
            var response = await _httpClient.DeleteAsync($"api/roles/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Role delete failed. Status: {StatusCode}, Error: {ErrorContent}", 
                     response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("JWT token issue - Unauthorized (401). Check token expiration.");
                }
                
                return false;
            }

            _logger.LogInformation("Role {RoleId} deleted successfully", id);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException deleting role {RoleId}. Message: {Message}", id, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId} via API", id);
            return false;
        }
    }
}

