using CommonArchitecture.Core.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IDashboardApiService
{
    Task<DashboardStatsDto?> GetStatsAsync();
}
