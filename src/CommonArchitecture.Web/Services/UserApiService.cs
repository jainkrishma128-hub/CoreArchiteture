using CommonArchitecture.Application.DTOs;
using System.Net.Http.Headers;

namespace CommonArchitecture.Web.Services;

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiService> _logger;

    public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedResult<UserDto>> GetAllAsync(UserQueryParameters parameters)
    {
        try
        {
            // Build query string
            var queryString = $"api/users?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}&SortBy={parameters.SortBy}&SortOrder={parameters.SortOrder}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                queryString += $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            }

            var result = await _httpClient.GetFromJsonAsync<PaginatedResult<UserDto>>(queryString);
            return result ?? new PaginatedResult<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users from API");
            return new PaginatedResult<UserDto>();
        }
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId} from API", id);
            return null;
        }
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createDto, IFormFile? profileImage)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            
            formData.Add(new StringContent(createDto.Name), "Name");
            formData.Add(new StringContent(createDto.Email), "Email");
            formData.Add(new StringContent(createDto.Mobile), "Mobile");
            formData.Add(new StringContent(createDto.RoleId.ToString()), "RoleId");

            if (profileImage != null)
            {
                var fileContent = new StreamContent(profileImage.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(profileImage.ContentType);
                formData.Add(fileContent, "ProfileImage", profileImage.FileName);
            }

            var response = await _httpClient.PostAsync("api/users", formData);
            response.EnsureSuccessStatusCode();
            
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user ?? throw new Exception("Failed to deserialize created user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user via API");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto updateDto, IFormFile? profileImage)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            
            formData.Add(new StringContent(updateDto.Name), "Name");
            formData.Add(new StringContent(updateDto.Email), "Email");
            formData.Add(new StringContent(updateDto.Mobile), "Mobile");
            formData.Add(new StringContent(updateDto.RoleId.ToString()), "RoleId");
            
            if (!string.IsNullOrEmpty(updateDto.ProfileImagePath))
            {
                formData.Add(new StringContent(updateDto.ProfileImagePath), "ExistingProfileImagePath");
            }

            if (profileImage != null)
            {
                var fileContent = new StreamContent(profileImage.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(profileImage.ContentType);
                formData.Add(fileContent, "ProfileImage", profileImage.FileName);
            }

            var response = await _httpClient.PutAsync($"api/users/{id}", formData);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} via API", id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId} via API", id);
            return false;
        }
    }
}

