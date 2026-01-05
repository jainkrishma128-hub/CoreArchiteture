using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface ICategoryRepository
{
    IQueryable<Category> GetQuery();
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    void Add(Category category);
    void Update(Category category);
    void Delete(Category category);
}
