using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Infrastructure.Modules
{
    /// <summary>
    /// Defines a module for dependency injection registration.
    /// Modules organize related service registrations by concern.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Registers all services for this module.
        /// </summary>
        /// <param name="services">The service collection to register services into</param>
        /// <param name="configuration">The application configuration</param>
        void RegisterServices(IServiceCollection services, IConfiguration configuration);
    }
}
