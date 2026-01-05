using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IRoleMenuRepository
{
 Task<IEnumerable<RoleMenu>> GetAllAsync();
 Task<RoleMenu?> GetByIdAsync(int id);
 Task<RoleMenu?> GetByRoleAndMenuAsync(int roleId, int menuId);
 Task<IEnumerable<RoleMenu>> GetByRoleIdAsync(int roleId);
 Task<IEnumerable<RoleMenu>> GetByMenuIdAsync(int menuId);
 Task<RoleMenu> AddAsync(RoleMenu roleMenu);
 Task UpdateAsync(RoleMenu roleMenu);
 Task DeleteAsync(int id);
 Task DeleteByRoleAndMenuAsync(int roleId, int menuId);
 Task<IEnumerable<RoleMenu>> GetPagedAsync(int? roleId, string sortBy, string sortOrder, int pageNumber, int pageSize);
 Task<int> GetTotalCountAsync(int? roleId);
}
