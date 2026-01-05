using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CommonArchitecture.Web.Services;

public class TokenStorageService : ITokenStorageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<TokenStorageService> _logger;
    
    private const string ACCESS_TOKEN_COOKIE = "access_token";
    private const string REFRESH_TOKEN_COOKIE = "refresh_token";
    private const string TOKEN_EXPIRES_COOKIE = "token_expires";
    
    // Cache keys
    private const string ACCESS_TOKEN_CACHE_KEY = "cached_access_token";
    private const string REFRESH_TOKEN_CACHE_KEY = "cached_refresh_token";
    private const string TOKEN_EXPIRES_CACHE_KEY = "cached_token_expires";

    public TokenStorageService(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, ILogger<TokenStorageService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<string?> GetAccessTokenAsync()
    {
        _logger.LogInformation("=== GET ACCESS TOKEN ===");
        
        // First, try to get from HTTP context (user request)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request != null)
        {
            var cookieToken = httpContext.Request.Cookies[ACCESS_TOKEN_COOKIE];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogInformation("Found token in cookies");
                return Task.FromResult<string?>(cookieToken);
            }
            else
            {
                _logger.LogInformation("Token NOT found in cookies");
            }
        }
        else
        {
            _logger.LogInformation("HTTP Context is null");
        }

        // Fallback to memory cache (for background HTTP requests)
        _logger.LogInformation("Checking memory cache...");
        if (_memoryCache.TryGetValue(ACCESS_TOKEN_CACHE_KEY, out string? cachedToken))
        {
            _logger.LogInformation("Found token in memory cache!");
            return Task.FromResult(cachedToken);
        }
        else
        {
            _logger.LogWarning("Token NOT found in memory cache!");
        }

        return Task.FromResult<string?>(null);
    }

    public Task<string?> GetRefreshTokenAsync()
    {
        _logger.LogInformation("=== GET REFRESH TOKEN ===");
   
        // First, try to get from HTTP context (user request)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request != null)
        {
            var cookieToken = httpContext.Request.Cookies[REFRESH_TOKEN_COOKIE];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                _logger.LogInformation("Found refresh token in cookies");
                return Task.FromResult<string?>(cookieToken);
            }
        }

        // Fallback to memory cache (for background HTTP requests)
        if (_memoryCache.TryGetValue(REFRESH_TOKEN_CACHE_KEY, out string? cachedToken))
        {
            _logger.LogInformation("Found refresh token in memory cache!");
            return Task.FromResult(cachedToken);
        }

        _logger.LogWarning("Refresh token NOT found!");
        return Task.FromResult<string?>(null);
    }

    public Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        _logger.LogInformation("=== SAVING TOKENS ===");
        _logger.LogInformation("Access Token: {TokenPreview}...", accessToken?.Substring(0, Math.Min(20, accessToken?.Length ?? 0)));
        _logger.LogInformation("Expires At: {ExpiresAt}", expiresAt);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Response != null)
        {
            _logger.LogInformation("HTTP Context available, saving to cookies");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = expiresAt
            };

            // Access token cookie (shorter expiration)
            httpContext.Response.Cookies.Append(ACCESS_TOKEN_COOKIE, accessToken, cookieOptions);

            // Refresh token cookie (longer expiration - 7 days)
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            httpContext.Response.Cookies.Append(REFRESH_TOKEN_COOKIE, refreshToken, refreshCookieOptions);

            // Expiration timestamp cookie
            httpContext.Response.Cookies.Append(TOKEN_EXPIRES_COOKIE, expiresAt.ToString("O"), cookieOptions);
        }
        else
        {
            _logger.LogWarning("HTTP Context not available, skipping cookie storage");
        }

        // Always cache in memory for use by JwtTokenHandler
        _logger.LogInformation("Caching tokens in memory");
        _memoryCache.Set(ACCESS_TOKEN_CACHE_KEY, accessToken, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiresAt
        });

        _memoryCache.Set(REFRESH_TOKEN_CACHE_KEY, refreshToken, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.UtcNow.AddDays(7)
        });

        _memoryCache.Set(TOKEN_EXPIRES_CACHE_KEY, expiresAt, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.UtcNow.AddDays(7)
        });

        _logger.LogInformation("=== TOKENS SAVED ===");
        return Task.CompletedTask;
    }

    public Task ClearTokensAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Response != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            httpContext.Response.Cookies.Append(ACCESS_TOKEN_COOKIE, string.Empty, cookieOptions);
            httpContext.Response.Cookies.Append(REFRESH_TOKEN_COOKIE, string.Empty, cookieOptions);
            httpContext.Response.Cookies.Append(TOKEN_EXPIRES_COOKIE, string.Empty, cookieOptions);
        }

        // Clear from cache
        _memoryCache.Remove(ACCESS_TOKEN_CACHE_KEY);
        _memoryCache.Remove(REFRESH_TOKEN_CACHE_KEY);
        _memoryCache.Remove(TOKEN_EXPIRES_CACHE_KEY);

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenExpiredAsync()
    {
        _logger.LogInformation("=== CHECK TOKEN EXPIRATION ===");
     
        var httpContext = _httpContextAccessor.HttpContext;
        DateTime? expiresAt = null;

    // First, try to get from HTTP context
    if (httpContext?.Request != null)
      {
       var expiresAtString = httpContext.Request.Cookies[TOKEN_EXPIRES_COOKIE];
     if (!string.IsNullOrEmpty(expiresAtString) && DateTime.TryParse(expiresAtString, out var parsedTime))
       {
  expiresAt = parsedTime;
       _logger.LogInformation("Found expiration time in cookies: {ExpiresAt}", expiresAt);
 }
        }

        // Fallback to cache
        if (expiresAt == null)
  {
            _logger.LogInformation("Checking memory cache for expiration time");
            _memoryCache.TryGetValue(TOKEN_EXPIRES_CACHE_KEY, out DateTime cachedExpiresAt);
   if (cachedExpiresAt != default)
       {
                expiresAt = cachedExpiresAt;
 _logger.LogInformation("Found expiration time in cache: {ExpiresAt}", expiresAt);
            }
      }

        if (expiresAt == null)
        {
    _logger.LogWarning("No expiration time found, returning true (expired)");
   return Task.FromResult(true);
 }

        var isExpired = DateTime.UtcNow >= expiresAt;
        _logger.LogInformation("Token expired check: IsExpired={IsExpired}, Now={Now}, ExpiresAt={ExpiresAt}", 
            isExpired, DateTime.UtcNow, expiresAt);
        
    return Task.FromResult(isExpired);
    }
}

