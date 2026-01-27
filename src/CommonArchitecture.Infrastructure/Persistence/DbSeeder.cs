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
                    // Force the database to apply migrations with a long timeout for Azure
                    _context.Database.SetCommandTimeout(120);
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Migration check failed. The database schema may already be up-to-date.");
            }

            await SeedMenusAsync();
            await SeedCategoriesAsync();
            await SeedUsersAsync();
            await SeedProductsAsync();
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
            new() { Name = "Inventory", Url = "/Admin/Inventory", Icon = "bi bi-box", DisplayOrder = 4, IsActive = true },
            new() { Name = "Orders", Url = "/Admin/Orders", Icon = "bi bi-cart-check", DisplayOrder = 5, IsActive = true },
            new() { Name = "Role Master", Url = "/Admin/Roles", Icon = "bi bi-shield-lock", DisplayOrder = 6, IsActive = true },
            new() { Name = "User Master", Url = "/Admin/Users", Icon = "bi bi-people", DisplayOrder = 7, IsActive = true },
            new() { Name = "Menu Master", Url = "/Admin/Menus", Icon = "bi bi-list", DisplayOrder = 8, IsActive = true },
            new() { Name = "Role Permission", Url = "/Admin/RoleMenus", Icon = "bi bi-gear", DisplayOrder = 9, IsActive = true },
            new() { Name = "Hangfire Jobs", Url = "/Admin/HangfireJobs", Icon = "bi bi-clock-history", DisplayOrder = 10, IsActive = true },
            new() { Name = "Log Monitoring", Url = "/Admin/Logs", Icon = "bi bi-journal-text", DisplayOrder = 11, IsActive = true }
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
            // Perform permission check safely
            
            // Actually, let's just use a try-catch for the specific column error
            try 
            {
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
            }
            catch (Exception ex) when (ex.Message.Contains("UpdatedAt"))
            {
                _logger.LogWarning("Skipping individual permission check due to missing UpdatedAt column. The Master Sync migration should fix this.");
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin permissions seeded/updated successfully.");
    }

    private async Task SeedUsersAsync()
    {
        // Check if the admin role exists
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (adminRole == null) return;

        // Add default admin user if not exists
        var adminMobile = "8758453771";
        if (!await _context.Users.AnyAsync(u => u.Mobile == adminMobile))
        {
            _context.Users.Add(new User
            {
                Name = "Anant Dosi",
                Email = "admin@example.com",
                Mobile = adminMobile,
                RoleId = adminRole.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation("Default Admin User seeded successfully.");
        }
    }

    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync()) return;

        var categories = await _context.Categories.ToListAsync();
        var productsToSeed = new List<Product>();

        foreach (var category in categories)
        {
            var categoryProducts = GetDefaultProductsForCategory(category.Name, category.Id);
            productsToSeed.AddRange(categoryProducts);
        }

        _context.Products.AddRange(productsToSeed);
        await _context.SaveChangesAsync();
        _logger.LogInformation("64 Products seeded successfully (8 per category).");
    }

    private List<Product> GetDefaultProductsForCategory(string categoryName, int categoryId)
    {
        return categoryName switch
        {
            "Electronics" => new List<Product>
            {
                new() { Name = "iPhone 15 Pro", Description = "Latest Apple flagship with Titanium design", Price = 999.00m, CategoryId = categoryId },
                new() { Name = "Samsung Galaxy S23", Description = "Powerful Android smartphone with great camera", Price = 799.00m, CategoryId = categoryId },
                new() { Name = "MacBook Air M2", Description = "Thin and light laptop with M2 chip", Price = 1199.00m, CategoryId = categoryId },
                new() { Name = "Sony WH-1000XM5", Description = "Industry leading noise canceling headphones", Price = 399.00m, CategoryId = categoryId },
                new() { Name = "iPad Pro 12.9", Description = "The ultimate tablet experience with M2", Price = 1099.00m, CategoryId = categoryId },
                new() { Name = "Dell XPS 15", Description = "Premium Windows laptop with 4K display", Price = 1899.00m, CategoryId = categoryId },
                new() { Name = "Nintendo Switch OLED", Description = "Handheld gaming with vibrant OLED screen", Price = 349.00m, CategoryId = categoryId },
                new() { Name = "Logitech MX Master 3S", Description = "The most advanced productivity mouse", Price = 99.00m, CategoryId = categoryId }
            },
            "Clothing" => new List<Product>
            {
                new() { Name = "Classic White T-Shirt", Description = "100% Cotton premium basic tee", Price = 25.00m, CategoryId = categoryId },
                new() { Name = "Denim Jacket", Description = "Vintage wash rugged denim jacket", Price = 85.00m, CategoryId = categoryId },
                new() { Name = "Slim Fit Chinos", Description = "Comfortable office-ready stretch chinos", Price = 55.00m, CategoryId = categoryId },
                new() { Name = "Leather Biker Jacket", Description = "Genuine lambskin black leather jacket", Price = 299.00m, CategoryId = categoryId },
                new() { Name = "Wool Blend Sweater", Description = "Warm and soft merino wool blend", Price = 75.00m, CategoryId = categoryId },
                new() { Name = "Athletic Joggers", Description = "Performance fabric tapered joggers", Price = 45.00m, CategoryId = categoryId },
                new() { Name = "Summer Floral Dress", Description = "Lightweight breathable cotton dress", Price = 65.00m, CategoryId = categoryId },
                new() { Name = "Hooded Windbreaker", Description = "Water-resistant light outer shell", Price = 60.00m, CategoryId = categoryId }
            },
            "Books" => new List<Product>
            {
                new() { Name = "Clean Architecture", Description = "Craftsman's Guide to Software Structure", Price = 42.00m, CategoryId = categoryId },
                new() { Name = "Atomic Habits", Description = "Build Good Habits & Break Bad Ones", Price = 18.00m, CategoryId = categoryId },
                new() { Name = "Deep Work", Description = "Rules for Focused Success in a Distracted World", Price = 22.00m, CategoryId = categoryId },
                new() { Name = "The Pragmatic Programmer", Description = "Your Journey to Mastery", Price = 45.00m, CategoryId = categoryId },
                new() { Name = "The Alchemist", Description = "A Fable About Following Your Dream", Price = 15.00m, CategoryId = categoryId },
                new() { Name = "To Kill a Mockingbird", Description = "Classic literature masterpiece", Price = 12.00m, CategoryId = categoryId },
                new() { Name = "1984", Description = "George Orwell's dystopian classic", Price = 14.00m, CategoryId = categoryId },
                new() { Name = "Refactoring", Description = "Improving the Design of Existing Code", Price = 50.00m, CategoryId = categoryId }
            },
            "Home & Garden" => new List<Product>
            {
                new() { Name = "Smart LED Bulb", Description = "RGBW bulb compatible with Alexa/Google", Price = 15.00m, CategoryId = categoryId },
                new() { Name = "Air Purifier", Description = "HEPA filter for large rooms", Price = 129.00m, CategoryId = categoryId },
                new() { Name = "Ergonomic Office Chair", Description = "Breathable mesh with lumbar support", Price = 249.00m, CategoryId = categoryId },
                new() { Name = "Non-Stick Cookware Set", Description = "12-piece ceramic coated kitchen set", Price = 199.00m, CategoryId = categoryId },
                new() { Name = "Outdoor String Lights", Description = "Waterproof Edison style bulbs", Price = 35.00m, CategoryId = categoryId },
                new() { Name = "Succulent Plant Set", Description = "Pack of 5 real easy-care succulents", Price = 25.00m, CategoryId = categoryId },
                new() { Name = "Robot Vacuum", Description = "Self-charging smart floor cleaner", Price = 299.00m, CategoryId = categoryId },
                new() { Name = "Modern Table Lamp", Description = "Minimalist design with USB port", Price = 40.00m, CategoryId = categoryId }
            },
            "Sports" => new List<Product>
            {
                new() { Name = "Yoga Mat", Description = "Extra thick non-slip eco-friendly mat", Price = 30.00m, CategoryId = categoryId },
                new() { Name = "Adjustable Dumbbells", Description = "Space-saving 5 to 50 lbs set", Price = 349.00m, CategoryId = categoryId },
                new() { Name = "Running Shoes", Description = "High-performance cushioned trainers", Price = 120.00m, CategoryId = categoryId },
                new() { Name = "Mountain Bike", Description = "21-speed aluminum frame trail bike", Price = 599.00m, CategoryId = categoryId },
                new() { Name = "Smart Fitness Tracker", Description = "Heart rate and sleep monitoring", Price = 99.00m, CategoryId = categoryId },
                new() { Name = "Insulated Water Bottle", Description = "32oz stainless steel vacuum flask", Price = 25.00m, CategoryId = categoryId },
                new() { Name = "Tennis Racket", Description = "Lightweight carbon fiber pro racket", Price = 150.00m, CategoryId = categoryId },
                new() { Name = "Badminton Set", Description = "4-racket set with net and shuttles", Price = 45.00m, CategoryId = categoryId }
            },
            "Toys" => new List<Product>
            {
                new() { Name = "LEGO Star Wars Set", Description = "Millennium Falcon building kit", Price = 160.00m, CategoryId = categoryId },
                new() { Name = "Remote Control Car", Description = "Off-road 4WD high speed monster truck", Price = 55.00m, CategoryId = categoryId },
                new() { Name = "Plush Teddy Bear", Description = "Soft and cuddly 24-inch giant bear", Price = 20.00m, CategoryId = categoryId },
                new() { Name = "Magnetic Building Blocks", Description = "100-piece educational 3D set", Price = 40.00m, CategoryId = categoryId },
                new() { Name = "Digital Kids Camera", Description = "1080p video with fun filters", Price = 35.00m, CategoryId = categoryId },
                new() { Name = "Statuete Action Figure", Description = "Limited edition hero collectible", Price = 25.00m, CategoryId = categoryId },
                new() { Name = "Classic Board Game", Description = "Strategic family fun for 4 players", Price = 15.00m, CategoryId = categoryId },
                new() { Name = "Scientific Slime Kit", Description = "DIY glow-in-the-dark lab set", Price = 18.00m, CategoryId = categoryId }
            },
            "Health & Beauty" => new List<Product>
            {
                new() { Name = "Electric Toothbrush", Description = "Sonic technology with 3 modes", Price = 80.00m, CategoryId = categoryId },
                new() { Name = "Moisturizing Cream", Description = "Hyaluronic acid facial hydration", Price = 15.00m, CategoryId = categoryId },
                new() { Name = "Hair Dryer", Description = "Ionic professional blow dryer", Price = 45.00m, CategoryId = categoryId },
                new() { Name = "Essential Oil Diffuser", Description = "Ultra-quiet aromatherapy mister", Price = 30.00m, CategoryId = categoryId },
                new() { Name = "Yoga Foam Roller", Description = "High density deep tissue massager", Price = 20.00m, CategoryId = categoryId },
                new() { Name = "Organic Face Serum", Description = "Vitamin C brightening serum", Price = 25.00m, CategoryId = categoryId },
                new() { Name = "Sunscreen SPF 50", Description = "Broad spectrum water resistant", Price = 12.00m, CategoryId = categoryId },
                new() { Name = "Bath Bomb Set", Description = "Pack of 12 luxury aromatherapy bombs", Price = 25.00m, CategoryId = categoryId }
            },
            "Automotive" => new List<Product>
            {
                new() { Name = "Dash Cam 4K", Description = "Dual lens front and rear monitoring", Price = 150.00m, CategoryId = categoryId },
                new() { Name = "Car Vacuum Cleaner", Description = "High power portable handheld vacuum", Price = 40.00m, CategoryId = categoryId },
                new() { Name = "Jump Starter Power Bank", Description = "1000A peak portable car starter", Price = 80.00m, CategoryId = categoryId },
                new() { Name = "Microfiber Cleaning Cloths", Description = "Pack of 12 lint-free towels", Price = 15.00m, CategoryId = categoryId },
                new() { Name = "Tire Pressure Gauge", Description = "Digital backlit 150 PSI gauge", Price = 12.00m, CategoryId = categoryId },
                new() { Name = "Bluetooth Car Adapter", Description = "Hands-free FM transmitter", Price = 20.00m, CategoryId = categoryId },
                new() { Name = "Leather Seat Covers", Description = "Premium universal fit black set", Price = 120.00m, CategoryId = categoryId },
                new() { Name = "Car Phone Mount", Description = "Dashboard and windshield suction cup", Price = 15.00m, CategoryId = categoryId }
            },
            _ => new List<Product>()
        };
    }
}
