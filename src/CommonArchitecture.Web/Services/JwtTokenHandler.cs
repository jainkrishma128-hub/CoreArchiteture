using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CommonArchitecture.Web.Services;

public class JwtTokenHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenHandler> _logger;

    public JwtTokenHandler(
        ITokenStorageService tokenStorageService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<JwtTokenHandler> logger)
    {
        _tokenStorageService = tokenStorageService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== JWT HANDLER START ===");
        _logger.LogInformation("Request URL: {Url}", request.RequestUri);
    
        // Skip token for auth endpoints
        if (request.RequestUri?.AbsolutePath.Contains("/api/auth/") == true)
        {
            _logger.LogInformation("Skipping token for auth endpoint");
            return await base.SendAsync(request, cancellationToken);
        }

        // Get access token
        var accessToken = await _tokenStorageService.GetAccessTokenAsync();
        _logger.LogInformation("Access Token Retrieved: {HasToken}", !string.IsNullOrEmpty(accessToken));

        // Check if token is expired and try to refresh
        if (string.IsNullOrEmpty(accessToken) || await _tokenStorageService.IsTokenExpiredAsync())
        {
            _logger.LogWarning("Token is empty or expired, attempting refresh");
            accessToken = await RefreshTokenIfNeededAsync();
            _logger.LogInformation("Token after refresh: {HasToken}", !string.IsNullOrEmpty(accessToken));
        }

        // Add token to request if available
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _logger.LogInformation("Authorization header added");
        }
        else
        {
            _logger.LogError("NO TOKEN AVAILABLE - Request will be unauthorized!");
        }

        var response = await base.SendAsync(request, cancellationToken);
    
        _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);

        // If unauthorized, try to refresh token once and retry
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Got 401 Unauthorized, attempting token refresh and retry");
            var refreshedToken = await RefreshTokenIfNeededAsync();
            if (!string.IsNullOrEmpty(refreshedToken))
            {
                _logger.LogInformation("Retrying request with refreshed token");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshedToken);
                response = await base.SendAsync(request, cancellationToken);
                _logger.LogInformation("Retry Response Status: {StatusCode}", response.StatusCode);
            }
            else
            {
                _logger.LogError("Could not refresh token");
            }
        }

        _logger.LogInformation("=== JWT HANDLER END ===");
        return response;
    }

    private async Task<string?> RefreshTokenIfNeededAsync()
    {
        try
        {
            var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
            _logger.LogInformation("Refresh Token Retrieved: {HasToken}", !string.IsNullOrEmpty(refreshToken));
      
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogError("No refresh token available");
                return null;
            }

            // Create a new HttpClient for refresh token request (to avoid circular dependency)
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5089");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var refreshRequest = new CommonArchitecture.Application.DTOs.RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            };

            var json = JsonSerializer.Serialize(refreshRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
        
            _logger.LogInformation("Sending refresh token request to API");
            var response = await httpClient.PostAsync("/api/auth/refresh-token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Refresh Token Response Status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var refreshResponse = JsonSerializer.Deserialize<CommonArchitecture.Application.DTOs.RefreshTokenResponseDto>(
                    responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    
                if (refreshResponse?.Success == true && 
                    !string.IsNullOrEmpty(refreshResponse.AccessToken) && 
                    !string.IsNullOrEmpty(refreshResponse.RefreshToken))
                {
                    _logger.LogInformation("Token refreshed successfully");
                    await _tokenStorageService.SaveTokensAsync(
                        refreshResponse.AccessToken,
                        refreshResponse.RefreshToken,
                        refreshResponse.ExpiresAt ?? DateTime.UtcNow.AddMinutes(15));

                    return refreshResponse.AccessToken;
                }
                else
                {
                    _logger.LogError("Refresh response invalid: {@Response}", refreshResponse);
                }
            }
            else
            {
                _logger.LogError("Refresh token request failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
        }

        return null;
    }
}

