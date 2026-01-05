using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role> AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(int id);
    Task<IEnumerable<Role>> GetPagedAsync(string? searchTerm, string sortBy, string sortOrder, int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync(string? searchTerm);
}

