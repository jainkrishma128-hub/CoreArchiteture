-- Database Seeding Script for CommonArchitecture
-- This script populates the database with sample data for Products, Roles, and Users

USE CommonArchitectureDb;
GO

-- =============================================
-- 1. SEED ROLES
-- =============================================
PRINT 'Seeding Roles...';

-- Clear existing roles (optional - comment out if you want to keep existing data)
-- DELETE FROM Users;
-- DELETE FROM Roles;

-- Insert Roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Roles ON;
    INSERT INTO Roles (Id, RoleName, CreatedAt)
    VALUES (1, 'Admin', GETDATE());
    SET IDENTITY_INSERT Roles OFF;
    PRINT 'Admin role created';
END
ELSE
BEGIN
    PRINT 'Admin role already exists';
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 2)
BEGIN
    SET IDENTITY_INSERT Roles ON;
    INSERT INTO Roles (Id, RoleName, CreatedAt)
    VALUES (2, 'Product Manager', GETDATE());
    SET IDENTITY_INSERT Roles OFF;
    PRINT 'Product Manager role created';
END
ELSE
BEGIN
    PRINT 'Product Manager role already exists';
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 3)
BEGIN
    INSERT INTO Roles (RoleName, CreatedAt)
    VALUES ('Customer', GETDATE());
    PRINT 'Customer role created';
END

-- =============================================
-- 2. SEED USERS
-- =============================================
PRINT '';
PRINT 'Seeding Users...';

-- Create test admin user
IF NOT EXISTS (SELECT 1 FROM Users WHERE Mobile = '9876543210')
BEGIN
    INSERT INTO Users (Name, Email, Mobile, RoleId, CreatedAt)
    VALUES ('Admin User', 'admin@example.com', '9876543210', 1, GETDATE());
    PRINT 'Admin user created - Mobile: 9876543210, OTP: 1234';
END
ELSE
BEGIN
    PRINT 'Admin user already exists - Mobile: 9876543210';
END

-- Create test product manager user
IF NOT EXISTS (SELECT 1 FROM Users WHERE Mobile = '9876543211')
BEGIN
    INSERT INTO Users (Name, Email, Mobile, RoleId, CreatedAt)
    VALUES ('Product Manager', 'pm@example.com', '9876543211', 2, GETDATE());
    PRINT 'Product Manager user created - Mobile: 9876543211, OTP: 1234';
END
ELSE
BEGIN
    PRINT 'Product Manager user already exists - Mobile: 9876543211';
END

-- =============================================
-- 3. SEED PRODUCTS
-- =============================================
PRINT '';
PRINT 'Seeding Products...';

-- Delete dummy product if exists
DELETE FROM Products WHERE Name = 'string';

-- Electronics Category
INSERT INTO Products (Name, Description, Price, Stock, CreatedAt)
VALUES 
    ('iPhone 15 Pro', 'Latest Apple smartphone with A17 Pro chip, 256GB storage, and ProMotion display', 999.99, 50, GETDATE()),
    ('Samsung Galaxy S24 Ultra', 'Premium Android smartphone with S Pen, 512GB storage, and 200MP camera', 1199.99, 35, GETDATE()),
    ('MacBook Pro 16"', 'Powerful laptop with M3 Max chip, 32GB RAM, and 1TB SSD', 2499.99, 20, GETDATE()),
    ('Dell XPS 15', 'High-performance laptop with Intel i9, 32GB RAM, and 4K OLED display', 1899.99, 25, GETDATE()),
    ('Sony WH-1000XM5', 'Premium noise-cancelling wireless headphones with 30-hour battery life', 399.99, 100, GETDATE()),
    ('AirPods Pro 2', 'Apple wireless earbuds with active noise cancellation and spatial audio', 249.99, 150, GETDATE()),
    ('iPad Air 11"', 'Versatile tablet with M2 chip, 256GB storage, and Apple Pencil support', 699.99, 60, GETDATE()),
    ('Samsung Galaxy Tab S9', 'Android tablet with S Pen, 128GB storage, and 120Hz display', 599.99, 45, GETDATE());

-- Home & Living Category
INSERT INTO Products (Name, Description, Price, Stock, CreatedAt)
VALUES 
    ('Dyson V15 Detect', 'Cordless vacuum cleaner with laser dust detection and LCD screen', 649.99, 30, GETDATE()),
    ('Ninja Air Fryer', '6-quart air fryer with 6-in-1 functionality and digital controls', 129.99, 80, GETDATE()),
    ('Keurig K-Elite', 'Single-serve coffee maker with iced coffee capability and 75oz reservoir', 169.99, 55, GETDATE()),
    ('iRobot Roomba j7+', 'Smart robot vacuum with self-emptying base and obstacle avoidance', 799.99, 25, GETDATE());

-- Fashion & Accessories
INSERT INTO Products (Name, Description, Price, Stock, CreatedAt)
VALUES 
    ('Nike Air Max 270', 'Comfortable running shoes with Max Air cushioning and breathable mesh', 149.99, 120, GETDATE()),
    ('Adidas Ultraboost 23', 'Premium running shoes with Boost cushioning and Primeknit upper', 189.99, 90, GETDATE()),
    ('Ray-Ban Aviator', 'Classic sunglasses with UV protection and metal frame', 179.99, 75, GETDATE()),
    ('Apple Watch Series 9', 'Advanced smartwatch with health tracking, GPS, and always-on display', 429.99, 65, GETDATE()),
    ('Fossil Gen 6', 'Stylish smartwatch with Wear OS, heart rate monitoring, and GPS', 299.99, 40, GETDATE());

-- Gaming & Entertainment
INSERT INTO Products (Name, Description, Price, Stock, CreatedAt)
VALUES 
    ('PlayStation 5', 'Next-gen gaming console with 4K gaming and ultra-fast SSD', 499.99, 15, GETDATE()),
    ('Xbox Series X', 'Powerful gaming console with 4K/120fps gaming and Game Pass', 499.99, 18, GETDATE()),
    ('Nintendo Switch OLED', 'Hybrid gaming console with vibrant OLED screen and Joy-Con controllers', 349.99, 50, GETDATE()),
    ('Meta Quest 3', 'Advanced VR headset with mixed reality and 128GB storage', 499.99, 30, GETDATE()),
    ('Logitech G Pro X', 'Professional gaming keyboard with mechanical switches and RGB lighting', 149.99, 70, GETDATE());

-- Sports & Fitness
INSERT INTO Products (Name, Description, Price, Stock, CreatedAt)
VALUES 
    ('Peloton Bike+', 'Premium exercise bike with rotating screen and auto-resistance', 2495.00, 10, GETDATE()),
    ('Bowflex Dumbbells', 'Adjustable dumbbells set (5-52.5 lbs) with compact design', 349.99, 35, GETDATE()),
    ('Fitbit Charge 6', 'Advanced fitness tracker with heart rate monitoring and GPS', 159.99, 85, GETDATE()),
    ('Garmin Forerunner 965', 'Premium GPS running watch with AMOLED display and training metrics', 599.99, 25, GETDATE());

PRINT '';
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' products inserted successfully';

-- =============================================
-- 4. DISPLAY SUMMARY
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'DATABASE SEEDING COMPLETED';
PRINT '========================================';
PRINT '';

-- Display counts
DECLARE @RoleCount INT, @UserCount INT, @ProductCount INT;
SELECT @RoleCount = COUNT(*) FROM Roles;
SELECT @UserCount = COUNT(*) FROM Users;
SELECT @ProductCount = COUNT(*) FROM Products;

PRINT 'Total Roles: ' + CAST(@RoleCount AS VARCHAR);
PRINT 'Total Users: ' + CAST(@UserCount AS VARCHAR);
PRINT 'Total Products: ' + CAST(@ProductCount AS VARCHAR);
PRINT '';

-- Display sample data
PRINT 'Sample Products:';
SELECT TOP 5 Id, Name, Price, Stock FROM Products ORDER BY Id;

PRINT '';
PRINT 'User Credentials for Testing:';
PRINT '  Admin - Mobile: 9876543210, OTP: 1234';
PRINT '  Product Manager - Mobile: 9876543211, OTP: 1234';
PRINT '';
PRINT 'You can now access the Admin Panel and Shop with populated data!';
GO
