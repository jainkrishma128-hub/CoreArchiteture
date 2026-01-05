using CommonArchitecture.Core.Entities;

namespace CommonArchitecture.Core.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<IEnumerable<Product>> GetPagedAsync(string? searchTerm, string sortBy, string sortOrder, int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync(string? searchTerm);
    Task BulkAddAsync(IEnumerable<Product> products);
}
