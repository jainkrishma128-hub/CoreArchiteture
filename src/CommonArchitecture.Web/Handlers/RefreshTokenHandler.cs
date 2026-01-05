using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CommonArchitecture.Web.Handlers
{
 public class RefreshTokenHandler : DelegatingHandler
 {
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<RefreshTokenHandler> _logger;
  private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1,1);

  public RefreshTokenHandler(IServiceProvider serviceProvider, ILogger<RefreshTokenHandler> logger)
  {
   _serviceProvider = serviceProvider;
   _logger = logger;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
   _logger.LogDebug("RefreshTokenHandler: Sending request {Method} {Url}", request.Method, request.RequestUri);

   // Resolve scoped services from the service provider
   using var scope = _serviceProvider.CreateScope();
   var tokenStorageService = scope.ServiceProvider.GetRequiredService<ITokenStorageService>();
   var authApiService = scope.ServiceProvider.GetRequiredService<IAuthApiService>();

   // Attach access token if present
   var accessToken = await tokenStorageService.GetAccessTokenAsync();
   if (!string.IsNullOrEmpty(accessToken))
   {
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    _logger.LogDebug("Attached access token to request");
   }
   else
   {
    _logger.LogDebug("No access token available to attach");
   }

   // Buffer request content so we can retry if needed
   byte[]? contentBytes = null;
   if (request.Content != null)
   {
    try
    {
     contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
     _logger.LogDebug("Buffered request content ({Length} bytes)", contentBytes?.Length ??0);
    }
    catch (System.Exception ex)
    {
     _logger.LogWarning(ex, "Failed to buffer request content; retries may fail");
    }
   }

   var response = await base.SendAsync(request, cancellationToken);

   if (response.StatusCode != HttpStatusCode.Unauthorized)
   {
    _logger.LogDebug("Response status {StatusCode} - returning without refresh", response.StatusCode);
    return response;
   }

   _logger.LogInformation("Received401 Unauthorized for {Url}; attempting refresh flow", request.RequestUri);

   //401 - try refresh once, ensure only one refresh runs at a time
   await _refreshLock.WaitAsync(cancellationToken);
   try
   {
    // Another thread might have refreshed the token while we waited
    var currentAccess = await tokenStorageService.GetAccessTokenAsync();
    if (!string.IsNullOrEmpty(currentAccess))
    {
     _logger.LogDebug("Found an updated access token while waiting for lock, retrying request");
     var retryWithCurrent = CloneRequest(request, contentBytes);
     retryWithCurrent.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentAccess);
     var retryResp = await base.SendAsync(retryWithCurrent, cancellationToken);
     if (retryResp.StatusCode != HttpStatusCode.Unauthorized)
     {
      _logger.LogInformation("Retry with updated token succeeded ({StatusCode})", retryResp.StatusCode);
      return retryResp;
     }
     _logger.LogDebug("Retry with updated token still unauthorized");
    }

    // Perform refresh
    var refreshToken = await tokenStorageService.GetRefreshTokenAsync();
    if (string.IsNullOrEmpty(refreshToken))
    {
     _logger.LogWarning("No refresh token available; cannot refresh");
     return response; // can't refresh
    }

    _logger.LogInformation("Calling API to refresh token");
    var refreshResp = await authApiService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = refreshToken });
    if (refreshResp == null)
    {
     _logger.LogWarning("Refresh API returned null");
     return response; // refresh failed
    }

    if (!refreshResp.Success || string.IsNullOrEmpty(refreshResp.AccessToken))
    {
     _logger.LogWarning("Refresh API unsuccessful or did not return access token: Success={Success}", refreshResp.Success);
     return response; // refresh failed
    }

    _logger.LogInformation("Token refreshed successfully; saving tokens and retrying original request");

    // Save new tokens
    await tokenStorageService.SaveTokensAsync(
     refreshResp.AccessToken,
     refreshResp.RefreshToken ?? refreshToken,
     refreshResp.ExpiresAt ?? DateTime.UtcNow.AddMinutes(15));

    // Retry original request with new token
    var retry = CloneRequest(request, contentBytes);
    retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResp.AccessToken);
    var finalResp = await base.SendAsync(retry, cancellationToken);
    _logger.LogInformation("Final retry response status: {StatusCode}", finalResp.StatusCode);
    return finalResp;
   }
   finally
   {
    _refreshLock.Release();
    _logger.LogDebug("Released refresh lock");
   }
  }

  private static HttpRequestMessage CloneRequest(HttpRequestMessage request, byte[]? contentBytes)
  {
   var clone = new HttpRequestMessage(request.Method, request.RequestUri)
   {
    Version = request.Version
   };

   // Copy content if present
   if (contentBytes != null)
   {
    clone.Content = new ByteArrayContent(contentBytes);
    if (request.Content?.Headers != null)
    {
     foreach (var header in request.Content.Headers)
     {
      clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
     }
    }
   }

   // Copy headers
   foreach (var header in request.Headers)
   {
    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
   }

   return clone;
  }
 }
}
