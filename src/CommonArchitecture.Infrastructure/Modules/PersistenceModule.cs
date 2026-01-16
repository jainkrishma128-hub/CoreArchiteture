using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using CommonArchitecture.Infrastructure.Repositories;
using CommonArchitecture.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CommonArchitecture.Infrastructure.Modules
{
    /// <summary>
    /// Registers all persistence and data access services.
    /// Includes database context, repositories, and unit of work pattern.
    /// </summary>
    public class PersistenceModule : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Validate connection string
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured. " +
                    "Check appsettings.json or environment variables.");

            // Register pooled DbContext factory
            services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                    sqlOptions.CommandTimeout(30);
                }));

            // Register scoped ApplicationDbContext that uses the factory
            services.AddScoped<ApplicationDbContext>(provider =>
                provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                    .CreateDbContext());

            // Register repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IRoleMenuRepository, RoleMenuRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        }
    }
}
