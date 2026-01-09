-- =============================================
-- Insert 5 Products for Each Category
-- =============================================

DECLARE @ElectronicsId INT, @ClothingId INT, @BooksId INT, @HomeGardenId INT;
DECLARE @SportsId INT, @ToysId INT, @HealthBeautyId INT, @AutomotiveId INT;

-- Get Category IDs
SELECT @ElectronicsId = Id FROM Categories WHERE Name = 'Electronics';
SELECT @ClothingId = Id FROM Categories WHERE Name = 'Clothing';
SELECT @BooksId = Id FROM Categories WHERE Name = 'Books';
SELECT @HomeGardenId = Id FROM Categories WHERE Name = 'Home & Garden';
SELECT @SportsId = Id FROM Categories WHERE Name = 'Sports';
SELECT @ToysId = Id FROM Categories WHERE Name = 'Toys';
SELECT @HealthBeautyId = Id FROM Categories WHERE Name = 'Health & Beauty';
SELECT @AutomotiveId = Id FROM Categories WHERE Name = 'Automotive';

PRINT 'Seeding 5 products for each category...';

-- =============================================
-- ELECTRONICS (5 products)
-- =============================================
IF @ElectronicsId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'iPhone 15 Pro', 'Latest Apple smartphone with A17 Pro chip, 256GB storage, and ProMotion display', 999.99, 50, @ElectronicsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'iPhone 15 Pro' AND CategoryId = @ElectronicsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Samsung Galaxy S24 Ultra', 'Premium Android smartphone with S Pen, 512GB storage, and 200MP camera', 1199.99, 35, @ElectronicsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Samsung Galaxy S24 Ultra' AND CategoryId = @ElectronicsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'MacBook Pro 16"', 'Powerful laptop with M3 Max chip, 32GB RAM, and 1TB SSD storage', 2499.99, 20, @ElectronicsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'MacBook Pro 16"' AND CategoryId = @ElectronicsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Sony WH-1000XM5', 'Premium noise-cancelling wireless headphones with 30-hour battery life', 399.99, 100, @ElectronicsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Sony WH-1000XM5' AND CategoryId = @ElectronicsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'iPad Air 11"', 'Versatile tablet with M2 chip, 256GB storage, and Apple Pencil support', 699.99, 60, @ElectronicsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'iPad Air 11"' AND CategoryId = @ElectronicsId);
    
    PRINT '✓ Electronics: 5 products inserted';
END

-- =============================================
-- CLOTHING (5 products)
-- =============================================
IF @ClothingId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Premium Cotton T-Shirt', 'High-quality 100% cotton t-shirt available in multiple colors', 29.99, 150, @ClothingId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Premium Cotton T-Shirt' AND CategoryId = @ClothingId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Slim Fit Jeans', 'Classic slim fit jeans with stretch fabric and perfect fit', 79.99, 120, @ClothingId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Slim Fit Jeans' AND CategoryId = @ClothingId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Winter Jacket', 'Warm insulated winter jacket with water-resistant outer layer', 199.99, 45, @ClothingId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Winter Jacket' AND CategoryId = @ClothingId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Casual Dress Shirt', 'Comfortable casual dress shirt perfect for work or casual wear', 59.99, 80, @ClothingId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Casual Dress Shirt' AND CategoryId = @ClothingId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Summer Shorts', 'Lightweight summer shorts in khaki and navy colors', 39.99, 110, @ClothingId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Summer Shorts' AND CategoryId = @ClothingId);
    
    PRINT '✓ Clothing: 5 products inserted';
END

-- =============================================
-- BOOKS (5 products)
-- =============================================
IF @BooksId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'The Art of Programming', 'Comprehensive guide to programming concepts and best practices', 49.99, 200, @BooksId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'The Art of Programming' AND CategoryId = @BooksId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Digital Marketing Masterclass', 'Complete course in digital marketing strategies and tools', 39.99, 180, @BooksId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Digital Marketing Masterclass' AND CategoryId = @BooksId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Business Strategy Guide', 'Essential strategies for building and scaling your business', 59.99, 150, @BooksId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Business Strategy Guide' AND CategoryId = @BooksId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Web Design Fundamentals', 'Learn modern web design principles and user experience', 44.99, 170, @BooksId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Web Design Fundamentals' AND CategoryId = @BooksId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Cloud Computing Essentials', 'Master cloud architecture and deployment strategies', 54.99, 140, @BooksId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Cloud Computing Essentials' AND CategoryId = @BooksId);
    
    PRINT '✓ Books: 5 products inserted';
END

-- =============================================
-- HOME & GARDEN (5 products)
-- =============================================
IF @HomeGardenId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Robot Vacuum Cleaner', 'Smart robot vacuum with app control and scheduling', 299.99, 40, @HomeGardenId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Robot Vacuum Cleaner' AND CategoryId = @HomeGardenId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Smart LED Light Bulbs', 'WiFi-enabled LED bulbs with color control and dimming', 24.99, 300, @HomeGardenId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Smart LED Light Bulbs' AND CategoryId = @HomeGardenId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Air Purifier', 'HEPA air purifier for bedroom and living room', 179.99, 55, @HomeGardenId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Air Purifier' AND CategoryId = @HomeGardenId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Garden Tool Set', 'Complete garden tool set with ergonomic handles', 79.99, 85, @HomeGardenId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Garden Tool Set' AND CategoryId = @HomeGardenId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Electric Coffee Maker', 'Programmable coffee maker with thermal carafe', 89.99, 70, @HomeGardenId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Electric Coffee Maker' AND CategoryId = @HomeGardenId);
    
    PRINT '✓ Home & Garden: 5 products inserted';
END

-- =============================================
-- SPORTS (5 products)
-- =============================================
IF @SportsId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Professional Basketball', 'Official size basketball with leather construction', 69.99, 95, @SportsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Professional Basketball' AND CategoryId = @SportsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Yoga Mat Premium', 'Non-slip yoga mat with carrying strap and storage bag', 49.99, 130, @SportsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Yoga Mat Premium' AND CategoryId = @SportsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Adjustable Dumbbells', 'Set of adjustable dumbbells from 5 to 50 lbs', 399.99, 30, @SportsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Adjustable Dumbbells' AND CategoryId = @SportsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Running Shoes Elite', 'Professional grade running shoes with cushioning technology', 159.99, 110, @SportsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Running Shoes Elite' AND CategoryId = @SportsId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Fitness Tracker Watch', 'Advanced fitness tracker with heart rate monitor', 199.99, 75, @SportsId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Fitness Tracker Watch' AND CategoryId = @SportsId);
    
    PRINT '✓ Sports: 5 products inserted';
END

-- =============================================
-- TOYS (5 products)
-- =============================================
IF @ToysId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Building Block Set', 'Creative building blocks set with 500+ pieces', 39.99, 160, @ToysId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Building Block Set' AND CategoryId = @ToysId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Remote Control Car', 'High-speed RC car with 4WD and all-terrain capability', 79.99, 90, @ToysId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Remote Control Car' AND CategoryId = @ToysId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Educational Robot Kit', 'Programmable robot kit for STEM learning', 99.99, 70, @ToysId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Educational Robot Kit' AND CategoryId = @ToysId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Puzzle Collection', 'Set of 5 puzzles with varying difficulty levels', 34.99, 140, @ToysId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Puzzle Collection' AND CategoryId = @ToysId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Interactive Board Games', 'Popular board games for family entertainment', 54.99, 105, @ToysId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Interactive Board Games' AND CategoryId = @ToysId);
    
    PRINT '✓ Toys: 5 products inserted';
END

-- =============================================
-- HEALTH & BEAUTY (5 products)
-- =============================================
IF @HealthBeautyId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Facial Skincare Set', 'Complete facial skincare set with cleanser and moisturizer', 79.99, 85, @HealthBeautyId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Facial Skincare Set' AND CategoryId = @HealthBeautyId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Hair Care Bundle', 'Premium hair care bundle with shampoo and conditioner', 49.99, 120, @HealthBeautyId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Hair Care Bundle' AND CategoryId = @HealthBeautyId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Vitamin Supplements', 'Daily multivitamin and mineral supplement pack', 29.99, 200, @HealthBeautyId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Vitamin Supplements' AND CategoryId = @HealthBeautyId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Electric Toothbrush', 'Smart electric toothbrush with multiple modes', 69.99, 95, @HealthBeautyId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Electric Toothbrush' AND CategoryId = @HealthBeautyId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Fitness Scale Smart', 'Digital smart scale with body composition analysis', 99.99, 65, @HealthBeautyId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Fitness Scale Smart' AND CategoryId = @HealthBeautyId);
    
    PRINT '✓ Health & Beauty: 5 products inserted';
END

-- =============================================
-- AUTOMOTIVE (5 products)
-- =============================================
IF @AutomotiveId IS NOT NULL
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Car Dash Camera', '1080p car dash camera with night vision and loop recording', 149.99, 70, @AutomotiveId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Car Dash Camera' AND CategoryId = @AutomotiveId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Oil Filter Set', 'Premium oil filters for regular maintenance', 34.99, 200, @AutomotiveId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Oil Filter Set' AND CategoryId = @AutomotiveId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Car Seat Covers', 'Protective car seat covers with easy installation', 89.99, 95, @AutomotiveId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Car Seat Covers' AND CategoryId = @AutomotiveId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Portable Air Compressor', 'Electric portable air compressor for tire inflation', 79.99, 110, @AutomotiveId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Portable Air Compressor' AND CategoryId = @AutomotiveId);
    
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt)
    SELECT 'Car Cleaning Kit', 'Complete car detailing and cleaning kit', 59.99, 130, @AutomotiveId, GETUTCDATE()
    WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Car Cleaning Kit' AND CategoryId = @AutomotiveId);
    
    PRINT '✓ Automotive: 5 products inserted';
END

PRINT '';
PRINT '========================================';
PRINT 'Product Seeding Completed Successfully!';
PRINT '========================================';
PRINT '';

-- Display summary
DECLARE @TotalProducts INT;
SELECT @TotalProducts = COUNT(*) FROM Products;
PRINT 'Total Products in Database: ' + CAST(@TotalProducts AS VARCHAR);
PRINT '';

-- Display products by category
PRINT 'Products by Category:';
SELECT c.Name, COUNT(p.Id) as ProductCount
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
GROUP BY c.Name
ORDER BY c.Name;

GO
