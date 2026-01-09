using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Infrastructure.Modules
{
    /// <summary>
    /// Registers all infrastructure-level application services.
    /// Includes logging, notifications, and other cross-cutting concerns.
    /// </summary>
    public class ApplicationServicesModule : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register logging service (scoped for request-specific logging)
            services.AddScoped<ILoggingService, LoggingService>();

            // Register notification service
            services.AddScoped<INotificationService, NotificationService>();
        }
    }
}
