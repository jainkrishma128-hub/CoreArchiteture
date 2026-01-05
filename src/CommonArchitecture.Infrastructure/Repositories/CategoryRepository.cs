using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommonArchitecture.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Category> GetQuery()
    {
     return _context.Categories.AsQueryable();
    }

    public async Task<List<Category>> GetAllAsync()
    {
   return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
  return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public void Add(Category category)
    {
        _context.Categories.Add(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
     _context.Categories.Remove(category);
 }
}
