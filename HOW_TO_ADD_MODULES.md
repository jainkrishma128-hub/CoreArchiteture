# How to Add New Modules to Your Architecture

## Quick Guide: Adding Modules Safely & Securely

This guide shows you how to add new features/modules to your application using the modular DI system.

---

## Template: Basic Module

```csharp
using CommonArchitecture.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Infrastructure.Modules
{
    /// <summary>
    /// Registers [Module Name] services.
    /// </summary>
    public class [ModuleName]Module : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // 1. Validate configuration (SECURITY)
            var requiredSetting = configuration["[Section]:[Key]"];
            if (string.IsNullOrWhiteSpace(requiredSetting))
                throw new InvalidOperationException(
                    "[Section]:[Key] is not configured. Check appsettings.json or environment variables.");

            // 2. Register services
            services.AddScoped<IServiceInterface, ServiceImplementation>();
            
            // 3. Configure options if needed
            services.Configure<[Options]>(configuration.GetSection("[Section]"));
        }
    }
}
```

---

## Example 1: Email Service Module

```csharp
using CommonArchitecture.Core.Modules;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CommonArchitecture.Infrastructure.Modules
{
    public class EmailModule : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Validate email configuration
            var emailConfig = configuration.GetSection("Email");
            var smtpServer = emailConfig["SmtpServer"];
            var fromEmail = emailConfig["FromEmail"];
            
            if (string.IsNullOrWhiteSpace(smtpServer))
                throw new InvalidOperationException(
                    "Email:SmtpServer not configured. Set in appsettings.json or env vars.");
            
            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException(
                    "Email:FromEmail not configured. Set in appsettings.json or env vars.");

            // Register email service
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}
```

**Update appsettings.json:**
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "noreply@yourapp.com",
    "Username": "${EMAIL_USERNAME}",
    "Password": "${EMAIL_PASSWORD}"
  }
}
```

**Update Program.cs:**
```csharp
var modules = new IModule[]
{
    new PersistenceModule(),
    new ApplicationServicesModule(),
    new CachingModule(),
    new EmailModule()  // ← Add here
};
builder.Services.AddModules(builder.Configuration, modules);
```

---

## Example 2: SMS Service Module (with API Key)

```csharp
public class SmsModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var smsConfig = configuration.GetSection("Sms");
        var apiKey = smsConfig["ApiKey"];
        var apiUrl = smsConfig["ApiUrl"];
        
        // Security: Validate sensitive config
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "SMS:ApiKey must be set via environment variable or User Secrets, not appsettings.json!");
        
        if (string.IsNullOrWhiteSpace(apiUrl))
            throw new InvalidOperationException(
                "SMS:ApiUrl not configured.");

        // Register SMS service
        services.AddScoped<ISmsService, SmsService>();
        
        // Configure SMS options
        services.Configure<SmsOptions>(options =>
        {
            options.ApiKey = apiKey;
            options.ApiUrl = apiUrl;
        });
    }
}
```

**Security Best Practice:**
```json
// appsettings.json (DON'T put secrets here!)
{
  "Sms": {
    "ApiUrl": "https://api.sms-provider.com"
  }
}

// User Secrets (for development):
// dotnet user-secrets set "Sms:ApiKey" "your-secret-key"

// Production Environment Variables:
// SMS_APIKEY=your-prod-secret-key
```

---

## Example 3: File Storage Module (Complex)

```csharp
using Amazon.S3;
using CommonArchitecture.Core.Modules;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Infrastructure.Modules
{
    public class FileStorageModule : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var storageType = configuration["FileStorage:Type"]?.ToLower() ?? "local";
            
            switch (storageType)
            {
                case "s3":
                    RegisterS3Storage(services, configuration);
                    break;
                case "azure":
                    RegisterAzureStorage(services, configuration);
                    break;
                case "local":
                default:
                    RegisterLocalStorage(services, configuration);
                    break;
            }
        }

        private void RegisterS3Storage(IServiceCollection services, IConfiguration configuration)
        {
            var awsConfig = configuration.GetSection("FileStorage:Aws");
            var accessKey = awsConfig["AccessKey"];
            var secretKey = awsConfig["SecretKey"];
            var bucketName = awsConfig["BucketName"];

            // Validate
            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException(
                    "AWS credentials not configured. Use User Secrets or env vars.");

            // Configure AWS S3
            var awsCredentials = new BasicAWSCredentials(accessKey, secretKey);
            services.AddSingleton<IAmazonS3>(new AmazonS3Client(
                awsCredentials,
                RegionEndpoint.GetBySystemName(awsConfig["Region"] ?? "us-east-1")));

            services.AddScoped<IFileStorageService, S3FileStorageService>();
        }

        private void RegisterAzureStorage(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["FileStorage:Azure:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Azure connection string not configured.");

            services.AddScoped<IFileStorageService, AzureFileStorageService>();
        }

        private void RegisterLocalStorage(IServiceCollection services, IConfiguration configuration)
        {
            var uploadPath = configuration["FileStorage:Local:UploadPath"] ?? "uploads";
            
            services.AddScoped<IFileStorageService>(provider =>
                new LocalFileStorageService(uploadPath));
        }
    }
}
```

---

## Example 4: Background Jobs Module (Hangfire)

```csharp
using CommonArchitecture.Core.Modules;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Infrastructure.Modules
{
    public class BackgroundJobsModule : IModule
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("DefaultConnection not configured");

            // Configure Hangfire
            services.AddHangfire(x => x
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();
        }
    }
}
```

**Register in Program.cs:**
```csharp
var modules = new IModule[]
{
    // ... other modules ...
    new BackgroundJobsModule()
};
builder.Services.AddModules(builder.Configuration, modules);

// If using Hangfire, add dashboard:
app.MapHangfireDashboard("/hangfire");
```

---

## Testing Modules

```csharp
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonArchitecture.Tests
{
    public class EmailModuleTests
    {
        [Fact]
        public void RegisterServices_WithValidConfig_Succeeds()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Email:SmtpServer", "smtp.test.com" },
                    { "Email:FromEmail", "test@test.com" }
                })
                .Build();

            var services = new ServiceCollection();
            var module = new EmailModule();

            // Act
            module.RegisterServices(services, config);
            var provider = services.BuildServiceProvider();

            // Assert
            var emailService = provider.GetService<IEmailService>();
            Assert.NotNull(emailService);
        }

        [Fact]
        public void RegisterServices_WithoutSmtpServer_ThrowsException()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            var services = new ServiceCollection();
            var module = new EmailModule();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                module.RegisterServices(services, config));
        }
    }
}
```

---

## Best Practices for Module Development

### 1. ✅ Always Validate Configuration
```csharp
if (string.IsNullOrWhiteSpace(requiredSetting))
    throw new InvalidOperationException($"{name} not configured");
```

### 2. ✅ Document Your Module
```csharp
/// <summary>
/// Registers email service with SMTP configuration.
/// Requires: Email:SmtpServer, Email:FromEmail in configuration.
/// </summary>
public class EmailModule : IModule { }
```

### 3. ✅ Use Appropriate Lifetimes
- **Singleton** - Stateless, shared across requests (caches, factories)
- **Scoped** - Per HTTP request (DbContext, business services)
- **Transient** - New instance each time (DTOs, helpers)

### 4. ✅ Handle Sensitive Data Safely
```csharp
// ❌ WRONG - Never in appsettings.json
"ApiKey": "secret123"

// ✅ RIGHT - Use User Secrets (dev) or Env Vars (prod)
// Development: dotnet user-secrets set "ApiKey" "secret123"
// Production: export API_KEY=secret123
```

### 5. ✅ Keep Modules Single-Purpose
Each module should handle one concern:
- `PersistenceModule` - Database only
- `EmailModule` - Email only
- `CachingModule` - Caching only

---

## Adding Module to Both API and Web Projects

For modules needed by both projects, create them once and reuse:

**API Program.cs:**
```csharp
var apiModules = new IModule[]
{
    new PersistenceModule(),
    new SharedApplicationServicesModule(),  // Shared
    new EmailModule(),
    new ApiSpecificModule()
};
builder.Services.AddModules(builder.Configuration, apiModules);
```

**Web Program.cs:**
```csharp
var webModules = new IModule[]
{
    new PersistenceModule(),
    new SharedApplicationServicesModule(),  // Shared
    new EmailModule(),
    new WebSpecificModule()
};
builder.Services.AddModules(builder.Configuration, webModules);
```

---

## Summary

✅ Create modules for new features
✅ Validate configuration early
✅ Keep modules single-purpose
✅ Use appropriate lifetimes
✅ Protect sensitive data
✅ Test your modules
✅ Document requirements

Your modular architecture is now ready to scale!
