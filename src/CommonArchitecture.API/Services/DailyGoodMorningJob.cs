using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommonArchitecture.API.Services;

public class DailyGoodMorningJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyGoodMorningJob> _logger;

    public DailyGoodMorningJob(IServiceProvider serviceProvider, ILogger<DailyGoodMorningJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendDailyGoodMorningMessagesAsync()
    {
        _logger.LogInformation("Starting daily Good Morning message job at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Get all active users
            var users = await context.Users
                .Where(u => !string.IsNullOrEmpty(u.Email) || !string.IsNullOrEmpty(u.Mobile))
                .ToListAsync();

            _logger.LogInformation("Found {Count} users to send Good Morning messages to", users.Count);

            int successCount = 0;
            int failureCount = 0;

            foreach (var user in users)
            {
                try
                {
                    await notificationService.SendGoodMorningMessageAsync(
                        user.Name,
                        user.Email ?? string.Empty,
                        user.Mobile ?? string.Empty
                    );
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send Good Morning message to user {UserId} ({Email})", 
                        user.Id, user.Email);
                    failureCount++;
                }
            }

            _logger.LogInformation(
                "Daily Good Morning message job completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in daily Good Morning message job");
            throw;
        }
    }
}

