using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CommonArchitecture.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Get the directory of the API project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "CommonArchitecture.API");
        
        // If we are currently in a subdirectory, climb up or adjust (EF tools usually run from the solution root or project root)
        if (!Directory.Exists(basePath))
        {
            // Try parent directory if we're inside CommonArchitecture.Infrastructure
            basePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "CommonArchitecture.API");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
