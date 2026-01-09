using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommonArchitecture.Infrastructure.Modules;
using System;
using System.Collections.Generic;

namespace CommonArchitecture.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for registering modules into the dependency injection container.
    /// Provides a clean, modular approach to service registration.
    /// </summary>
    public static class ModuleExtensions
    {
        /// <summary>
        /// Registers all modules and validates configuration.
        /// </summary>
        public static IServiceCollection AddModules(
            this IServiceCollection services,
            IConfiguration configuration,
            params IModule[] modules)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (modules == null || modules.Length == 0)
                throw new ArgumentException("At least one module must be provided", nameof(modules));

            foreach (var module in modules)
            {
                module.RegisterServices(services, configuration);
            }

            return services;
        }

        /// <summary>
        /// Registers modules from a collection.
        /// </summary>
        public static IServiceCollection AddModules(
            this IServiceCollection services,
            IConfiguration configuration,
            IEnumerable<IModule> modules)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (modules == null)
                throw new ArgumentNullException(nameof(modules));

            foreach (var module in modules)
            {
                module.RegisterServices(services, configuration);
            }

            return services;
        }
    }
}
