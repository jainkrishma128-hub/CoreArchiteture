using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface ILogApiService
{
    Task<PaginatedResult<LogDto>> GetAllAsync(LogQueryParameters parameters);
    Task<LogDto?> GetByIdAsync(int id);
}
