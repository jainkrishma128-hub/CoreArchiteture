using CommonArchitecture.Application.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Infrastructure.Modules
{
    /// <summary>
    /// Registers all caching-related services.
    /// Includes memory cache and cache invalidation helpers.
    /// </summary>
    public class CachingModule : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register memory cache
            services.AddMemoryCache();

            // Register caching helpers
            services.AddSingleton<CacheHelper>();
            services.AddScoped<CacheInvalidator>();
        }
    }
}
