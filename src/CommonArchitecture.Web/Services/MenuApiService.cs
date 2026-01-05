using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;

namespace CommonArchitecture.Web.Services;

public class MenuApiService : IMenuApiService
{
 private readonly HttpClient _httpClient;
 private readonly ILogger<MenuApiService> _logger;

 public MenuApiService(HttpClient httpClient, ILogger<MenuApiService> logger)
 {
 _httpClient = httpClient;
 _logger = logger;
 }

 public async Task<PaginatedResult<MenuDto>> GetAllAsync(MenuQueryParameters parameters)
 {
 try
 {
 var queryString = $"api/menus?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}&SortBy={parameters.SortBy}&SortOrder={parameters.SortOrder}";
 
 if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
 {
 queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
 }

 var result = await _httpClient.GetFromJsonAsync<PaginatedResult<MenuDto>>(queryString);
 return result ?? new PaginatedResult<MenuDto>();
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching menus from API");
 return new PaginatedResult<MenuDto>();
 }
 }

 public async Task<MenuDto?> GetByIdAsync(int id)
 {
 try
 {
 return await _httpClient.GetFromJsonAsync<MenuDto>($"api/menus/{id}");
 }
 catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
 {
 return null;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching menu {MenuId} from API", id);
 return null;
 }
 }

 public async Task<MenuDto> CreateAsync(CreateMenuDto createDto)
 {
 try
 {
 var response = await _httpClient.PostAsJsonAsync("api/menus", createDto);
 response.EnsureSuccessStatusCode();
 
 var menu = await response.Content.ReadFromJsonAsync<MenuDto>();
 return menu ?? throw new Exception("Failed to deserialize created menu");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error creating menu via API");
 throw;
 }
 }

 public async Task<bool> UpdateAsync(int id, UpdateMenuDto updateDto)
 {
 try
 {
 var response = await _httpClient.PutAsJsonAsync($"api/menus/{id}", updateDto);
 
 if (!response.IsSuccessStatusCode)
 {
 var errorContent = await response.Content.ReadAsStringAsync();
 _logger.LogError("Menu update failed. Status: {StatusCode}, Error: {ErrorContent}", 
 response.StatusCode, errorContent);
 return false;
 }

 return true;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error updating menu {MenuId} via API", id);
 return false;
 }
 }

 public async Task<bool> DeleteAsync(int id)
 {
 try
 {
 var response = await _httpClient.DeleteAsync($"api/menus/{id}");
 
 if (!response.IsSuccessStatusCode)
 {
 var errorContent = await response.Content.ReadAsStringAsync();
 _logger.LogError("Menu delete failed. Status: {StatusCode}, Error: {ErrorContent}", 
 response.StatusCode, errorContent);
 return false;
 }

 return true;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error deleting menu {MenuId} via API", id);
 return false;
 }
 }
}
