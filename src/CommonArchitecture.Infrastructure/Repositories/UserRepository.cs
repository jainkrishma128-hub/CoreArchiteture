using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using CommonArchitecture.Core.DTOs;
using System.Linq.Dynamic.Core;

namespace CommonArchitecture.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    // Whitelist of allowed sort fields to prevent injection attacks
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(User.Id),
        nameof(User.Name),
        nameof(User.Email),
        nameof(User.Mobile),
        nameof(User.CreatedAt)
    };

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.Include(u => u.Role).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        // SaveChanges removed - UnitOfWork is responsible for persisting
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        // SaveChanges removed - UnitOfWork is responsible for persisting
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            // SaveChanges removed - UnitOfWork is responsible for persisting
        }
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional search and sorting.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by name, email, or mobile</param>
    /// <param name="sortBy">Field name to sort by (Id, Name, Email, Mobile, CreatedAt)</param>
    /// <param name="sortOrder">Sort order: "asc" or "desc"</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of users</returns>
    public async Task<IEnumerable<User>> GetPagedAsync(string? searchTerm, string sortBy, string sortOrder, int pageNumber, int pageSize)
    {
        var query = _context.Users.Include(u => u.Role).AsQueryable();

        // Apply search filter
        query = ApplySearchFilter(query, searchTerm);

        // Apply dynamic sorting using System.Linq.Dynamic.Core
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return users;
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm)
    {
        var query = _context.Users.AsQueryable();

        // Apply search filter
        query = ApplySearchFilter(query, searchTerm);

        // No need to include Role for count query - optimization
        return await query.CountAsync();
    }

    /// <summary>
    /// Applies search filter to the query.
    /// Uses database collation (CI_AS in SQL Server) for case-insensitive search.
    /// This allows proper index usage instead of forcing LOWER() which prevents index usage.
    /// </summary>
    private static IQueryable<User> ApplySearchFilter(IQueryable<User> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Rely on database collation (CI_AS in SQL Server) for case-insensitive search
            // This allows proper index usage instead of forcing LOWER() which prevents index usage
            query = query.Where(u => 
                u.Name.Contains(searchTerm) || 
                u.Email.Contains(searchTerm) ||
                u.Mobile.Contains(searchTerm));
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
    private static IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, string sortOrder)
    {
        // Validate and sanitize sort field
        if (string.IsNullOrWhiteSpace(sortBy) || !AllowedSortFields.Contains(sortBy))
        {
            // Default to Id if invalid field
            sortBy = nameof(User.Id);
        }

        // Validate sort order
        var isDescending = sortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false;

        // Build dynamic OrderBy expression
        // System.Linq.Dynamic.Core allows us to use string-based ordering
        var orderByExpression = $"{sortBy} {(isDescending ? "descending" : "ascending")}";
        
        return query.OrderBy(orderByExpression);
    }

    public async Task<List<DailyStatDto>> GetDailyRegistrationsAsync(DateTime from, DateTime to)
    {
        return await _context.Users
            .Where(u => u.CreatedAt >= from && u.CreatedAt <= to)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new DailyStatDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(s => s.Date)
            .ToListAsync();
    }
}

