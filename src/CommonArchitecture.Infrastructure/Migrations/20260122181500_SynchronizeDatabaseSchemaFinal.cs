using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SynchronizeDatabaseSchemaFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === MASTER SCHEMA REPAIR (Forces all missing columns to exist) ===

            // 1. RoleMenus (The main culprit for the current crash)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RoleMenus]') AND type in (N'U'))
                    CREATE TABLE [RoleMenus] ([Id] int NOT NULL IDENTITY, [RoleId] int NOT NULL, [MenuId] int NOT NULL, [CanCreate] bit NOT NULL DEFAULT 0, [CanRead] bit NOT NULL DEFAULT 0, [CanUpdate] bit NOT NULL DEFAULT 0, [CanDelete] bit NOT NULL DEFAULT 0, [CanExecute] bit NOT NULL DEFAULT 0, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_RoleMenus] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RoleMenus]') AND name = 'UpdatedAt') 
                    ALTER TABLE [RoleMenus] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 2. Roles
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Roles]') AND type in (N'U'))
                    CREATE TABLE [Roles] ([Id] int NOT NULL IDENTITY, [RoleName] nvarchar(128) NOT NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'UpdatedAt') 
                    ALTER TABLE [Roles] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 3. Menus
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Menus]') AND type in (N'U'))
                    CREATE TABLE [Menus] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(128) NOT NULL, [Icon] nvarchar(64) NOT NULL, [Url] nvarchar(256) NOT NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_Menus] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Menus]') AND name = 'UpdatedAt') 
                    ALTER TABLE [Menus] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 4. Categories
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Categories]') AND type in (N'U'))
                    CREATE TABLE [Categories] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(256) NOT NULL, [Description] nvarchar(1000) NOT NULL, [IsActive] bit NOT NULL DEFAULT 1, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Categories]') AND name = 'UpdatedAt') 
                    ALTER TABLE [Categories] ADD [UpdatedAt] datetime2 NULL;
            ");

            // 5. Products
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Products]') AND type in (N'U'))
                    CREATE TABLE [Products] ([Id] int NOT NULL IDENTITY, [Name] nvarchar(256) NOT NULL, [Price] decimal(18,2) NOT NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_Products] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'UpdatedAt') 
                    ALTER TABLE [Products] ADD [UpdatedAt] datetime2 NULL;
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Products]') AND name = 'CategoryId') 
                    ALTER TABLE [Products] ADD [CategoryId] int NOT NULL DEFAULT 0;
            ");

            // 6. RefreshTokens (Fix all columns)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND type in (N'U'))
                    CREATE TABLE [RefreshTokens] ([Id] int NOT NULL IDENTITY, [Token] nvarchar(500) NOT NULL, [UserId] int NOT NULL, [ExpiresAt] datetime2 NOT NULL, [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()), CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]));
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'DeviceFingerprint') ALTER TABLE [RefreshTokens] ADD [DeviceFingerprint] nvarchar(256) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'UserAgent') ALTER TABLE [RefreshTokens] ADD [UserAgent] nvarchar(500) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'PreviousToken') ALTER TABLE [RefreshTokens] ADD [PreviousToken] nvarchar(500) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[RefreshTokens]') AND name = 'IpAddress') ALTER TABLE [RefreshTokens] ADD [IpAddress] nvarchar(45) NULL;
            ");

            // === SEED DATA REPAIR ===
            migrationBuilder.Sql(@"
                -- Ensure Admin Role
                IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin') 
                    INSERT INTO Roles (RoleName, CreatedAt) VALUES ('Admin', GETUTCDATE());
                
                DECLARE @AdminId int = (SELECT TOP 1 Id FROM Roles WHERE RoleName = 'Admin');

                -- Ensure Basic Menus
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Dashboard') INSERT INTO Menus (Name, Url, Icon, CreatedAt) VALUES ('Dashboard', '/Admin/Dashboard', 'bi bi-speedometer2', GETUTCDATE());
                IF NOT EXISTS (SELECT 1 FROM Menus WHERE Name = 'Products') INSERT INTO Menus (Name, Url, Icon, CreatedAt) VALUES ('Products', '/Admin/Products', 'bi bi-box-seam', GETUTCDATE());
                
                -- Ensure Admin User exists
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Mobile = '8758453771')
                    INSERT INTO Users (Name, Email, Mobile, RoleId, CreatedAt) 
                    VALUES ('Anant Dosi', 'admin@example.com', '8758453771', @AdminId, GETUTCDATE());

                -- Ensure Admin has permissions
                INSERT INTO RoleMenus (RoleId, MenuId, CanCreate, CanRead, CanUpdate, CanDelete, CanExecute, CreatedAt)
                SELECT @AdminId, m.Id, 1, 1, 1, 1, 1, GETUTCDATE()
                FROM Menus m
                WHERE NOT EXISTS (SELECT 1 FROM RoleMenus rm WHERE rm.RoleId = @AdminId AND rm.MenuId = m.Id);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
