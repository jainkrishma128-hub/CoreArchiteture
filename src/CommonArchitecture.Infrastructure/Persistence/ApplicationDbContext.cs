using CommonArchitecture.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommonArchitecture.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Logging tables
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<RequestResponseLog> RequestResponseLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
      
        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(256);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Configure relationship with Category
            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance optimization: Add indexes for common queries
            entity.HasIndex(p => p.CategoryId).HasDatabaseName("IX_Product_CategoryId");
            entity.HasIndex(p => p.Price).HasDatabaseName("IX_Product_Price");
            // Composite index for common filtered queries (category + price searches)
            entity.HasIndex(p => new { p.CategoryId, p.Price }).HasDatabaseName("IX_Product_CategoryId_Price");
            // Index for search queries on Name
            entity.HasIndex(p => p.Name).HasDatabaseName("IX_Product_Name");
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(256);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Performance optimization: Add indexes for common queries
            entity.HasIndex(c => c.IsActive).HasDatabaseName("IX_Category_IsActive");
            entity.HasIndex(c => new { c.IsActive, c.Name }).HasDatabaseName("IX_Category_IsActive_Name");
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RoleName).IsRequired().HasMaxLength(128);
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(r => r.RoleName).IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Mobile).HasMaxLength(20);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Mobile);

            // Configure relationship with Role
            entity.HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.DeviceFingerprint).HasMaxLength(256);
            entity.Property(rt => rt.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(rt => rt.UserAgent).HasMaxLength(500);
            entity.Property(rt => rt.PreviousToken).HasMaxLength(500);
            entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            entity.HasIndex(rt => new { rt.UserId, rt.DeviceFingerprint });

            // Configure relationship with User
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Menu entity
        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(128);
            entity.Property(m => m.Icon).HasMaxLength(64);
            entity.Property(m => m.Url).IsRequired().HasMaxLength(256);
            entity.Property(m => m.DisplayOrder).HasDefaultValue(0);
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(m => m.DisplayOrder);

            // Self-referencing relationship for parent-child menus
            entity.HasOne(m => m.ParentMenu)
                .WithMany(m => m.SubMenus)
                .HasForeignKey(m => m.ParentMenuId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure RoleMenu entity
        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(rm => rm.Id);
            entity.Property(rm => rm.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(rm => new { rm.RoleId, rm.MenuId }).IsUnique();

            // Configure relationships
            entity.HasOne(rm => rm.Role)
                .WithMany()
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rm => rm.Menu)
                .WithMany()
                .HasForeignKey(rm => rm.MenuId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ErrorLog
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.StackTrace).HasMaxLength(4000);
            entity.Property(e => e.Path).HasMaxLength(1024);
            entity.Property(e => e.Method).HasMaxLength(16);
            entity.Property(e => e.QueryString).HasMaxLength(2000);
            entity.Property(e => e.UserId).HasMaxLength(128);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure RequestResponseLog
        modelBuilder.Entity<RequestResponseLog>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Method).HasMaxLength(16);
            entity.Property(r => r.Path).HasMaxLength(1024);
            entity.Property(r => r.QueryString).HasMaxLength(2000);
            entity.Property(r => r.RequestBody).HasMaxLength(8000);
            entity.Property(r => r.ResponseBody).HasMaxLength(8000);
            entity.Property(r => r.IpAddress).HasMaxLength(45);
            entity.Property(r => r.UserAgent).HasMaxLength(1000);
            entity.Property(r => r.UserId).HasMaxLength(128);
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Optimization: Add indexes for sorting/filtering and dashboard stats queries
            entity.HasIndex(r => r.CreatedAt)
                .HasDatabaseName("IX_RequestResponseLogs_CreatedAt");
            entity.HasIndex(r => r.ResponseStatusCode)
                .HasDatabaseName("IX_RequestResponseLogs_ResponseStatusCode");
            entity.HasIndex(r => r.Method)
                .HasDatabaseName("IX_RequestResponseLogs_Method");
            
            // Composite index for dashboard stats filtering by date and status
            entity.HasIndex(r => new { r.CreatedAt, r.ResponseStatusCode })
                .HasDatabaseName("IX_RequestResponseLogs_CreatedAt_ResponseStatusCode");
        });

        // Configure InventoryTransaction entity
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.ToTable("InventoryTransactions");
            entity.HasKey(it => it.Id);
            entity.Property(it => it.Quantity).IsRequired();
            entity.Property(it => it.TransactionType).IsRequired();
            entity.Property(it => it.Reason).HasMaxLength(500);
            entity.Property(it => it.ReferenceNumber).HasMaxLength(100);
            entity.Property(it => it.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(it => it.Product)
                .WithMany()
                .HasForeignKey(it => it.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(it => it.ProductId);
            entity.HasIndex(it => it.TransactionType);
            entity.HasIndex(it => it.CreatedAt);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(256);
            entity.Property(o => o.Email).IsRequired().HasMaxLength(256);
            entity.Property(o => o.Phone).HasMaxLength(20);
            entity.Property(o => o.Address).IsRequired().HasMaxLength(500);
            entity.Property(o => o.City).IsRequired().HasMaxLength(100);
            entity.Property(o => o.ZipCode).IsRequired().HasMaxLength(20);
            entity.Property(o => o.Subtotal).HasPrecision(18, 2);
            entity.Property(o => o.Tax).HasPrecision(18, 2);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.OrderDate).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.OrderDate);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.ProductName).IsRequired().HasMaxLength(256);
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            entity.Property(oi => oi.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
