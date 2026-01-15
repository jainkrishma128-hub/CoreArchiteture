using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CommonArchitecture.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    // Whitelist of allowed sort fields to prevent injection attacks
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Product.Id),
        nameof(Product.Name),
        nameof(Product.Price),
        nameof(Product.CreatedAt)
    };

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        // SaveChanges removed - UnitOfWork is responsible for persisting
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        // SaveChanges removed - UnitOfWork is responsible for persisting
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            // SaveChanges removed - UnitOfWork is responsible for persisting
        }
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional search and sorting.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by name or description</param>
    /// <param name="sortBy">Field name to sort by (Id, Name, Price, Stock, CreatedAt)</param>
    /// <param name="sortOrder">Sort order: "asc" or "desc"</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of products</returns>
    public async Task<IEnumerable<Product>> GetPagedAsync(string? searchTerm, int? categoryId, string sortBy, string sortOrder, int pageNumber, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Apply filters
        query = ApplyFilters(query, searchTerm, categoryId);

        // Apply dynamic sorting using System.Linq.Dynamic.Core
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return products;
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm, int? categoryId)
    {
        var query = _context.Products.AsQueryable();

        // Apply filters
        query = ApplyFilters(query, searchTerm, categoryId);

        return await query.CountAsync();
    }

    public async Task BulkAddAsync(IEnumerable<Product> products)
    {
        await _context.Products.AddRangeAsync(products);
        // SaveChanges removed - UnitOfWork is responsible for persisting
    }

    /// <summary>
    /// Applies search filter to the query.
    /// Uses database collation (CI_AS in SQL Server) for case-insensitive search.
    /// This allows proper index usage instead of forcing LOWER() which prevents index usage.
    /// </summary>
    private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, string? searchTerm, int? categoryId)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Description.Contains(searchTerm));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return query;
    }

    /// <summary>
    /// Applies dynamic sorting to the query using System.Linq.Dynamic.Core.
    /// Validates sort field against whitelist to prevent injection attacks.
    /// </summary>
    /// <param name="query">The query to sort</param>
    /// <param name="sortBy">Field name to sort by</param>
    /// <param name="sortOrder">Sort order: "asc" or "desc"</param>
    /// <returns>Sorted query</returns>
    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortOrder)
    {
        // Validate and sanitize sort field
        if (string.IsNullOrWhiteSpace(sortBy) || !AllowedSortFields.Contains(sortBy))
        {
            // Default to Id if invalid field
            sortBy = nameof(Product.Id);
        }

        // Validate sort order
        var isDescending = sortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        // Build dynamic OrderBy expression
        // System.Linq.Dynamic.Core allows us to use string-based ordering
        var orderByExpression = $"{sortBy} {(isDescending ? "descending" : "ascending")}";
        
        return query.OrderBy(orderByExpression);
    }
}
