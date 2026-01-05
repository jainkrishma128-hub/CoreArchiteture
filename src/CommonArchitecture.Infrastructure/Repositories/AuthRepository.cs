using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommonArchitecture.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByMobileAsync(string mobile)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Mobile == mobile);
    }

    public async Task<User?> ValidateUserAsync(string mobile, int roleId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Mobile == mobile && u.RoleId == roleId);
    }
}
