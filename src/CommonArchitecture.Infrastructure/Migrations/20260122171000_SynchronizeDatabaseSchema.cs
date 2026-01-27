using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SynchronizeDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === MASTER SCHEMA FIXER (Ensures every table and every column exists) ===

            // 1. Roles
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Roles]') AND type in (N'U'))
                    CREATE TABLE [Roles] ([Id] int NOT NULL IDENTITY, [RoleName] nvarchar(128) NOT NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [UpdatedAt] datetime2 NULL, CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'UpdatedAt') ALTER TABLE [Roles] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 2. Users
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Users]') AND type in (N'U'))
                    CREATE TABLE [Users] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(256) NOT NULL, [Email] nvarchar(256) NOT NULL, [Mobile] nvarchar(20) NOT NULL, [RoleId] int NOT NULL, [ProfileImagePath] nvarchar(max) NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [UpdatedAt] datetime2 NULL, CONSTRAINT [PK_Users] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = 'UpdatedAt') ALTER TABLE [Users] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 3. Categories
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Categories]') AND type in (N'U'))
                    CREATE TABLE [Categories] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(256) NOT NULL, [Description] nvarchar(1000) NOT NULL, [IsActive] bit NOT NULL DEFAULT 1, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [UpdatedAt] datetime2 NULL, CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Categories]') AND name = 'UpdatedAt') ALTER TABLE [Categories] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 4. Products
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Products]') AND type in (N'U'))
                    CREATE TABLE [Products] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(256) NOT NULL, [Description] nvarchar(1000) NOT NULL, [Price] decimal(18, 2) NOT NULL, [CategoryId] int NOT NULL DEFAULT 0, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [UpdatedAt] datetime2 NULL, CONSTRAINT [PK_Products] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'CategoryId') ALTER TABLE [Products] ADD [CategoryId] int NOT NULL DEFAULT 0;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'UpdatedAt') ALTER TABLE [Products] ADD [UpdatedAt] datetime2 NULL;
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'Stock') ALTER TABLE [Products] DROP COLUMN [Stock];
            ");

            // 5. Menus
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Menus]') AND type in (N'U'))
                    CREATE TABLE [Menus] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(128) NOT NULL, [Icon] nvarchar(64) NOT NULL, [Url] nvarchar(256) NOT NULL, [ParentMenuId] int NULL, [DisplayOrder] int NOT NULL DEFAULT 0, [IsActive] bit NOT NULL DEFAULT 1, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [UpdatedAt] datetime2 NULL, CONSTRAINT [PK_Menus] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Menus]') AND name = 'UpdatedAt') ALTER TABLE [Menus] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 6. RoleMenus
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RoleMenus]') AND type in (N'U'))
                    CREATE TABLE [RoleMenus] ([Id] int NOT NULL IDENTITY, [RoleId] int NOT NULL, [MenuId] int NOT NULL, [CanCreate] bit NOT NULL DEFAULT 0, [CanRead] bit NOT NULL DEFAULT 0, [CanUpdate] bit NOT NULL DEFAULT 0, [CanDelete] bit NOT NULL DEFAULT 0, [CanExecute] bit NOT NULL DEFAULT 0, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [UpdatedAt] datetime2 NULL, CONSTRAINT [PK_RoleMenus] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RoleMenus]') AND name = 'UpdatedAt') ALTER TABLE [RoleMenus] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 7. RefreshTokens
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND type in (N'U'))
                    CREATE TABLE [RefreshTokens] ([Id] int NOT NULL IDENTITY, [UserId] int NOT NULL, [Token] nvarchar(500) NOT NULL, [ExpiresAt] datetime2 NOT NULL, [IsRevoked] bit NOT NULL DEFAULT 0, [RevokedAt] nvarchar(max) NULL, [IpAddress] nvarchar(45) NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]));
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'DeviceFingerprint') ALTER TABLE [RefreshTokens] ADD [DeviceFingerprint] nvarchar(256) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'UserAgent') ALTER TABLE [RefreshTokens] ADD [UserAgent] nvarchar(500) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'PreviousToken') ALTER TABLE [RefreshTokens] ADD [PreviousToken] nvarchar(500) NULL;
            ");

            // 8. Order & OrderItems Columns Fix
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Orders]') AND type in (N'U'))
                    CREATE TABLE [Orders] ([Id] int NOT NULL IDENTITY, [OrderNumber] nvarchar(50) NOT NULL, [OrderDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()), [CustomerName] nvarchar(256) NOT NULL, [Email] nvarchar(256) NOT NULL, [Phone] nvarchar(20) NOT NULL, [Address] nvarchar(500) NOT NULL, [City] nvarchar(100) NOT NULL, [ZipCode] nvarchar(20) NOT NULL, [Subtotal] decimal(18, 2) NOT NULL, [Tax] decimal(18, 2) NOT NULL, [TotalAmount] decimal(18, 2) NOT NULL, [Status] int NOT NULL DEFAULT 0, CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[OrderItems]') AND type in (N'U'))
                    CREATE TABLE [OrderItems] ([Id] int NOT NULL IDENTITY, [OrderId] int NOT NULL, [ProductId] int NOT NULL, [ProductName] nvarchar(256) NOT NULL, [UnitPrice] decimal(18, 2) NOT NULL, [Quantity] int NOT NULL, [TotalPrice] decimal(18, 2) NOT NULL, CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]));
            ");

            // === COMPREHENSIVE DATA SEEDING (Master Setup) ===
            migrationBuilder.Sql(@"
                -- 1. Ensure Admin Role
                IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin') 
                    INSERT INTO Roles (RoleName, CreatedAt) VALUES ('Admin', GETUTCDATE());
                
                DECLARE @AdminRoleId int = (SELECT TOP 1 Id FROM Roles WHERE RoleName = 'Admin');

                -- 2. Seed All Sidebar Menus
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Dashboard') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Dashboard', '/Admin/Dashboard', 'bi bi-speedometer2', 1, 1);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Products') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Products', '/Admin/Products', 'bi bi-box-seam', 1, 2);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Categories') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Categories', '/Admin/Categories', 'bi bi-tags', 1, 3);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Inventory') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Inventory', '/Admin/Inventory', 'bi bi-box', 1, 4);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Orders') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Orders', '/Admin/Orders', 'bi bi-cart-check', 1, 5);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Role Master') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Role Master', '/Admin/Roles', 'bi bi-shield-lock', 1, 6);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'User Master') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('User Master', '/Admin/Users', 'bi bi-people', 1, 7);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Menu Master') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Menu Master', '/Admin/Menus', 'bi bi-list', 1, 8);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Role Permission') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Role Permission', '/Admin/RoleMenus', 'bi bi-gear', 1, 9);
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Hangfire Jobs') INSERT INTO Menus (Name, Url, Icon, IsActive, DisplayOrder) VALUES ('Hangfire Jobs', '/Admin/HangfireJobs', 'bi bi-clock-history', 1, 10);

                -- 3. Grant Full Permissions to Admin
                INSERT INTO RoleMenus (RoleId, MenuId, CanCreate, CanRead, CanUpdate, CanDelete, CanExecute, CreatedAt)
                SELECT @AdminRoleId, m.Id, 1, 1, 1, 1, 1, GETUTCDATE()
                FROM Menus m
                WHERE NOT EXISTS (SELECT 1 FROM RoleMenus rm WHERE rm.RoleId = @AdminRoleId AND rm.MenuId = m.Id);

                -- 4. Seed Categories
                IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Electronics') INSERT INTO Categories (Name, Description, IsActive, CreatedAt) VALUES ('Electronics', 'Devices and Gadgets', 1, GETUTCDATE());
                IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Clothing') INSERT INTO Categories (Name, Description, IsActive, CreatedAt) VALUES ('Clothing', 'Fashion and Apparel', 1, GETUTCDATE());
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
