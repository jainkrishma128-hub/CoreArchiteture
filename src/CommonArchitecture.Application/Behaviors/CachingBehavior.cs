using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CommonArchitecture.Application.Behaviors;

public class CacheHelper
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheHelper> _logger;

    public CacheHelper(IMemoryCache cache, ILogger<CacheHelper> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public TResponse? Get<TResponse>(string key)
    {
        if (_cache.TryGetValue(key, out TResponse? cached))
        {
            _logger.LogInformation("Cache hit for {CacheKey}", key);
            return cached;
        }

        _logger.LogInformation("Cache miss for {CacheKey}", key);
        return default;
    }

    public void Set<TResponse>(string key, TResponse value, TimeSpan? slidingExpiration = null)
    {
        var options = new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(5) };
        _cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _logger.LogInformation("Invalidating cache for key: {CacheKey}", key);
        _cache.Remove(key);
    }
}
