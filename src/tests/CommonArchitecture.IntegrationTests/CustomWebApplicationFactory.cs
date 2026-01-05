using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace CommonArchitecture.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "SuperSecretKeyForTestingPurposesOnly123!" },
                { "Jwt:Issuer", "CommonArchitecture.API" },
                { "Jwt:Audience", "CommonArchitecture.Web" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(
                d => d.ServiceType.Name.Contains("DbContextOptions")).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        });
    }
}
