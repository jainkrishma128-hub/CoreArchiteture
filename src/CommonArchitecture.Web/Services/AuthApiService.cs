using CommonArchitecture.Application.DTOs;
using System.Text;
using System.Text.Json;

namespace CommonArchitecture.Web.Services;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthApiService(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<SendOtpResponseDto> SendOtpAsync(SendOtpRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/auth/send-otp", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<SendOtpResponseDto>(responseContent, _jsonOptions) 
                    ?? new SendOtpResponseDto { Success = false, Message = "Failed to parse response" };
            }

            var errorResponse = JsonSerializer.Deserialize<SendOtpResponseDto>(responseContent, _jsonOptions);
            return errorResponse ?? new SendOtpResponseDto { Success = false, Message = "Failed to send OTP" };
        }
        catch (Exception ex)
        {
            return new SendOtpResponseDto 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            };
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, _jsonOptions) 
                    ?? new LoginResponseDto { Success = false, Message = "Failed to parse response" };
                
                // Save tokens if login successful
                if (loginResponse.Success && 
                    !string.IsNullOrEmpty(loginResponse.AccessToken) && 
                    !string.IsNullOrEmpty(loginResponse.RefreshToken))
                {
                    await _tokenStorageService.SaveTokensAsync(
                        loginResponse.AccessToken,
                        loginResponse.RefreshToken,
                        loginResponse.ExpiresAt ?? DateTime.UtcNow.AddMinutes(15));
                }
                
                return loginResponse;
            }

            var errorResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, _jsonOptions);
            return errorResponse ?? new LoginResponseDto { Success = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            };
        }
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/auth/refresh-token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<RefreshTokenResponseDto>(responseContent, _jsonOptions) 
                    ?? new RefreshTokenResponseDto { Success = false, Message = "Failed to parse response" };
            }

            var errorResponse = JsonSerializer.Deserialize<RefreshTokenResponseDto>(responseContent, _jsonOptions);
            return errorResponse ?? new RefreshTokenResponseDto { Success = false, Message = "Token refresh failed" };
        }
        catch (Exception ex)
        {
            return new RefreshTokenResponseDto 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            };
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var request = new RefreshTokenRequestDto { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/logout", content);
                
                // Clear tokens regardless of response
                await _tokenStorageService.ClearTokensAsync();
                return response.IsSuccessStatusCode;
            }
            
            var response2 = await _httpClient.PostAsync("/api/auth/logout", null);
            await _tokenStorageService.ClearTokensAsync();
            return response2.IsSuccessStatusCode;
        }
        catch
        {
            await _tokenStorageService.ClearTokensAsync();
            return false;
        }
    }
}
