using CommonArchitecture.Core.DTOs;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;

    public DashboardService(IUnitOfWork unitOfWork, ILoggingService loggingService)
    {
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-30);

        var registrations = await _unitOfWork.Users.GetDailyRegistrationsAsync(from, to);
        var (apiCalls, statusDist, avgDuration) = await _loggingService.GetDashboardStatsAsync(from, to);

        return new DashboardStatsDto
        {
            UserRegistrations = registrations,
            ApiCalls = apiCalls,
            StatusCodes = statusDist,
            AverageResponseTime = avgDuration
        };
    }
}

