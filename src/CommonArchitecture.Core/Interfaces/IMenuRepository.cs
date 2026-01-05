using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IMenuRepository
{
 Task<IEnumerable<Menu>> GetAllAsync();
 Task<Menu?> GetByIdAsync(int id);
 Task<Menu> AddAsync(Menu menu);
 Task UpdateAsync(Menu menu);
 Task DeleteAsync(int id);
 Task<IEnumerable<Menu>> GetPagedAsync(string? searchTerm, string sortBy, string sortOrder, int pageNumber, int pageSize);
 Task<int> GetTotalCountAsync(string? searchTerm);
 Task<IEnumerable<Menu>> GetByParentIdAsync(int? parentMenuId);
}
