using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;

namespace CommonArchitecture.Web.Services;

public class RoleMenuApiService : IRoleMenuApiService
{
 private readonly HttpClient _httpClient;
 private readonly ILogger<RoleMenuApiService> _logger;

 public RoleMenuApiService(HttpClient httpClient, ILogger<RoleMenuApiService> logger)
 {
 _httpClient = httpClient;
 _logger = logger;
 }

 public async Task<RoleMenuPermissionsDto?> GetRolePermissionsAsync(int roleId)
 {
 try
 {
 var result = await _httpClient.GetFromJsonAsync<RoleMenuPermissionsDto>($"api/rolemenus/role/{roleId}");
 return result;
 }
 catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
 {
 return null;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error fetching role menu permissions for role {RoleId}", roleId);
 return null;
 }
 }

 public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<RoleMenuItemDto> menuPermissions)
 {
 try
 {
 var response = await _httpClient.PutAsJsonAsync($"api/rolemenus/role/{roleId}", menuPermissions);
 return response.IsSuccessStatusCode;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error updating role menu permissions for role {RoleId}", roleId);
 return false;
 }
 }
}
