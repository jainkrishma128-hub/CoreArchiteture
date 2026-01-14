using CommonArchitecture.Application.Services;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Extensions;
using CommonArchitecture.Infrastructure.Modules;
using CommonArchitecture.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Add User Secrets in Development
if (builder.Environment.IsDevelopment())
{
 builder.Configuration.AddUserSecrets<Program>();
}

// Add Environment Variables (takes precedence)
builder.Configuration.AddEnvironmentVariables();

// Register modules (modular service registration)
var modules = new IModule[]
{
    new PersistenceModule(),
    new ApplicationServicesModule(),
    new CachingModule()
};

builder.Services.AddModules(builder.Configuration, modules);

// Register application-specific services
builder.Services.AddScoped<IJwtService, JwtService>();

// Register application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<CommonArchitecture.Application.Services.IMenuService, CommonArchitecture.Application.Services.MenuService>();
builder.Services.AddScoped<CommonArchitecture.Application.Services.IRoleMenuService, CommonArchitecture.Application.Services.RoleMenuService>();

// Register background services
builder.Services.AddHostedService<CommonArchitecture.API.Services.RefreshTokenCleanupService>();
builder.Services.AddScoped<CommonArchitecture.API.Services.DailyGoodMorningJob>();

// Configure Hangfire
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(configuration => configuration
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

builder.Services.AddHangfireServer();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
 options.AddPolicy("AuthPolicy", context =>
 RateLimitPartition.GetFixedWindowLimiter(
 partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
 factory: partition => new FixedWindowRateLimiterOptions
 {
 AutoReplenishment = true,
 PermitLimit =5, //5 requests
 Window = TimeSpan.FromMinutes(1) // per minute
 }));
 
 options.OnRejected = async (context, cancellationToken) =>
 {
 context.HttpContext.Response.StatusCode =429;
 await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
 };
});

// Configure JWT Authentication
var secretKey = builder.Configuration["Jwt:SecretKey"] 
 ?? throw new InvalidOperationException("JWT SecretKey not configured. Set it in User Secrets or Environment Variables.");
var issuer = builder.Configuration["Jwt:Issuer"] ?? "CommonArchitecture.API";
var audience = builder.Configuration["Jwt:Audience"] ?? "CommonArchitecture.Web";

builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateLifetime = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = issuer,
 ValidAudience = audience,
 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
 ClockSkew = TimeSpan.Zero
 };
});

builder.Services.AddAuthorization();

// Configure CORS for Web layer
builder.Services.AddCors(options =>
{
 options.AddPolicy("WebPolicy", policy =>
 {
 policy.WithOrigins(
 "https://localhost:7001", // Web HTTPS
 "http://localhost:5000" // Web HTTP
 )
 .AllowAnyMethod()
 .AllowAnyHeader()
 .AllowCredentials(); // Required for cookies
 });
});

// Register application services
builder.Services.AddScoped<CommonArchitecture.Application.Services.IMenuService, CommonArchitecture.Application.Services.MenuService>();
builder.Services.AddScoped<CommonArchitecture.Application.Services.IRoleMenuService, CommonArchitecture.Application.Services.RoleMenuService>();

// Register caching helper
builder.Services.AddSingleton<CommonArchitecture.Application.Behaviors.CacheHelper>();
builder.Services.AddScoped<CommonArchitecture.Application.Behaviors.CacheInvalidator>();

// Add Controllers
builder.Services.AddControllers()
 .AddJsonOptions(options =>
 {
 options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve original property names
 });

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 app.MapOpenApi();
 app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("WebPolicy");

// Enable static files for uploaded images
app.UseStaticFiles();

// Request/Response logging middleware (early)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Rate Limiting middleware (must be before UseAuthentication)
app.UseRateLimiter();

// Exception handling middleware (should be before authentication/authorization)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure Hangfire Dashboard (API only - for monitoring)
// Note: In production, you may want to add authentication/authorization to this endpoint
if (app.Environment.IsDevelopment())
{
 app.MapHangfireDashboard("/hangfire", new DashboardOptions
 {
 DashboardTitle = "Hangfire Dashboard - API",
 Authorization = new[] { new CommonArchitecture.API.HangfireAuthorizationFilter() }
 });
}

// Schedule recurring jobs
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Schedule daily Good Morning message job at8:00 AM UTC (adjust timezone as needed)
// Note: Hangfire will resolve DailyGoodMorningJob from DI when the job runs
recurringJobManager.AddOrUpdate<CommonArchitecture.API.Services.DailyGoodMorningJob>(
 "daily-good-morning-messages",
 job => job.SendDailyGoodMorningMessagesAsync(),
 Cron.Daily(hour:8, minute:0),
 new RecurringJobOptions
 {
 TimeZone = TimeZoneInfo.Utc
 });

logger.LogInformation("Hangfire recurring job 'daily-good-morning-messages' scheduled for daily8:00 AM UTC");

var summaries = new[]
{
 "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
 var forecast = Enumerable.Range(1,5).Select(index =>
 new WeatherForecast
 (
 DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
 Random.Shared.Next(-20,55),
 summaries[Random.Shared.Next(summaries.Length)]
 ))
 .ToArray();
 return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
 public int TemperatureF =>32 + (int)(TemperatureC /0.5556);
}

public partial class Program { }
