using CommonArchitecture.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommonArchitecture.Infrastructure.Persistence;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(ApplicationDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Try to apply migrations, but handle cases where schema is already up-to-date
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Migration check failed. The database schema may already be up-to-date.");
            }

            await SeedMenusAsync();
            await SeedCategoriesAsync();
            await SeedRolePermissionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedMenusAsync()
    {
        var defaultMenus = new List<Menu>
        {
            new() { Name = "Dashboard", Url = "/Admin/Dashboard", Icon = "bi bi-speedometer2", DisplayOrder = 1, IsActive = true },
            new() { Name = "Products", Url = "/Admin/Products", Icon = "bi bi-box-seam", DisplayOrder = 2, IsActive = true },
            new() { Name = "Categories", Url = "/Admin/Categories", Icon = "bi bi-tags", DisplayOrder = 3, IsActive = true },
            new() { Name = "Role Master", Url = "/Admin/Roles", Icon = "bi bi-shield-lock", DisplayOrder = 4, IsActive = true },
            new() { Name = "User Master", Url = "/Admin/Users", Icon = "bi bi-people", DisplayOrder = 5, IsActive = true },
            new() { Name = "Menu Master", Url = "/Admin/Menus", Icon = "bi bi-list", DisplayOrder = 6, IsActive = true },
            new() { Name = "Role Permission", Url = "/Admin/RoleMenus", Icon = "bi bi-gear", DisplayOrder = 7, IsActive = true },
            new() { Name = "Hangfire Jobs", Url = "/Admin/HangfireJobs", Icon = "bi bi-clock-history", DisplayOrder = 8, IsActive = true }
        };

        foreach (var menu in defaultMenus)
        {
            if (!await _context.Menus.AnyAsync(m => m.Name == menu.Name))
            {
                _context.Menus.Add(menu);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Menus seeded/updated successfully.");
    }

    private async Task SeedCategoriesAsync()
    {
        var defaultCategories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Electronic devices and gadgets", IsActive = true },
            new() { Name = "Clothing", Description = "Apparel and fashion items", IsActive = true },
            new() { Name = "Books", Description = "Books and educational materials", IsActive = true },
            new() { Name = "Home & Garden", Description = "Home improvement and gardening supplies", IsActive = true },
            new() { Name = "Sports", Description = "Sports equipment and accessories", IsActive = true },
            new() { Name = "Toys", Description = "Toys and games for all ages", IsActive = true },
            new() { Name = "Health & Beauty", Description = "Health and beauty products", IsActive = true },
            new() { Name = "Automotive", Description = "Car parts and automotive accessories", IsActive = true }
        };

        foreach (var category in defaultCategories)
        {
            if (!await _context.Categories.AnyAsync(c => c.Name == category.Name))
            {
                _context.Categories.Add(category);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Categories seeded/updated successfully.");
    }

    private async Task SeedRolePermissionsAsync()
    {
        // Ensure Admin Role exists (It should be seeded by Identity or other means, but let's check basic role)
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
             // If no admin role, maybe we should create it? 
             // Assuming "Admin" role ID 1 is standard or exists. 
             // Let's create if not exists
             adminRole = new Role { RoleName = "Admin", CreatedAt = DateTime.UtcNow };
             _context.Roles.Add(adminRole);
             await _context.SaveChangesAsync();
        }

        var menus = await _context.Menus.ToListAsync();
        
        foreach (var menu in menus)
        {
            // Check if permission already exists
            var existingPermission = await _context.RoleMenus
                .FirstOrDefaultAsync(rm => rm.RoleId == adminRole.Id && rm.MenuId == menu.Id);

            if (existingPermission == null)
            {
                _context.RoleMenus.Add(new RoleMenu
                {
                    RoleId = adminRole.Id,
                    MenuId = menu.Id,
                    CanCreate = true,
                    CanRead = true,
                    CanUpdate = true,
                    CanDelete = true,
                    CanExecute = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Ensure Admin always has full rights (Self-Correcting)
                bool changed = false;
                if (!existingPermission.CanCreate) { existingPermission.CanCreate = true; changed = true; }
                if (!existingPermission.CanRead) { existingPermission.CanRead = true; changed = true; }
                if (!existingPermission.CanUpdate) { existingPermission.CanUpdate = true; changed = true; }
                if (!existingPermission.CanDelete) { existingPermission.CanDelete = true; changed = true; }
                if (!existingPermission.CanExecute) { existingPermission.CanExecute = true; changed = true; }
                
                if (changed) _context.RoleMenus.Update(existingPermission);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin permissions seeded/updated successfully.");
    }
}
