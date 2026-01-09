-- Create Categories table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(256) NOT NULL,
        Description NVARCHAR(1000),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2
    );
    
    -- Add foreign key to Products if it doesn't exist
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Products_Categories_CategoryId')
    BEGIN
        ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories_CategoryId 
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE;
    END
    
    -- Create index if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CategoryId')
    BEGIN
        CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
    END
END

-- Insert default categories if they don't exist
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Electronics')
BEGIN
    INSERT INTO Categories (Name, Description, IsActive) VALUES
        ('Electronics', 'Electronic devices and gadgets', 1),
        ('Clothing', 'Apparel and fashion items', 1),
        ('Books', 'Books and educational materials', 1),
        ('Home & Garden', 'Home improvement and gardening supplies', 1),
        ('Sports', 'Sports equipment and accessories', 1),
        ('Toys', 'Toys and games for all ages', 1),
        ('Health & Beauty', 'Health and beauty products', 1),
        ('Automotive', 'Car parts and automotive accessories', 1);
END

-- Now insert 5 products for each category
DECLARE @ElectronicsId INT = (SELECT Id FROM Categories WHERE Name = 'Electronics');
DECLARE @ClothingId INT = (SELECT Id FROM Categories WHERE Name = 'Clothing');
DECLARE @BooksId INT = (SELECT Id FROM Categories WHERE Name = 'Books');
DECLARE @HomeGardenId INT = (SELECT Id FROM Categories WHERE Name = 'Home & Garden');
DECLARE @SportsId INT = (SELECT Id FROM Categories WHERE Name = 'Sports');
DECLARE @ToysId INT = (SELECT Id FROM Categories WHERE Name = 'Toys');
DECLARE @HealthBeautyId INT = (SELECT Id FROM Categories WHERE Name = 'Health & Beauty');
DECLARE @AutomotiveId INT = (SELECT Id FROM Categories WHERE Name = 'Automotive');

-- Clear existing products to avoid duplicates
DELETE FROM Products;

-- Electronics Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Wireless Bluetooth Headphones', 'High-quality wireless headphones with noise cancellation and 30-hour battery life', 79.99, 150, @ElectronicsId, GETUTCDATE()),
    ('USB-C Fast Charging Cable', 'Durable USB-C cable supporting fast charging up to 100W', 12.99, 500, @ElectronicsId, GETUTCDATE()),
    ('4K Webcam with Microphone', 'Professional 4K webcam with built-in stereo microphone for streaming', 89.99, 85, @ElectronicsId, GETUTCDATE()),
    ('Portable Power Bank 20000mAh', 'Fast-charging power bank with dual USB ports and LED display', 39.99, 200, @ElectronicsId, GETUTCDATE()),
    ('Smart LED Light Bulbs (4-Pack)', 'WiFi-enabled RGB LED bulbs compatible with Alexa and Google Home', 49.99, 120, @ElectronicsId, GETUTCDATE());

-- Clothing Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Premium Cotton T-Shirt', 'Comfortable 100% cotton t-shirt in multiple colors, perfect for everyday wear', 19.99, 300, @ClothingId, GETUTCDATE()),
    ('Slim Fit Jeans', 'Classic slim fit denim jeans with stretch fabric for comfort', 59.99, 200, @ClothingId, GETUTCDATE()),
    ('Waterproof Winter Jacket', 'Insulated winter jacket with waterproof coating and thermal lining', 129.99, 80, @ClothingId, GETUTCDATE()),
    ('Athletic Running Shoes', 'Lightweight breathable running shoes with cushioned sole', 89.99, 150, @ClothingId, GETUTCDATE()),
    ('Casual Hoodie Sweatshirt', 'Soft fleece hoodie perfect for layering in colder weather', 44.99, 250, @ClothingId, GETUTCDATE());

-- Books Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('The Art of Programming', 'Comprehensive guide to software development and coding best practices', 39.99, 120, @BooksId, GETUTCDATE()),
    ('Digital Marketing Mastery', 'Complete course on digital marketing strategies and tools', 34.99, 100, @BooksId, GETUTCDATE()),
    ('Personal Finance Essentials', 'Learn how to manage money, invest, and build wealth', 24.99, 180, @BooksId, GETUTCDATE()),
    ('The Science of Happiness', 'Explore the psychology behind human happiness and well-being', 29.99, 95, @BooksId, GETUTCDATE()),
    ('Business Leadership Guide', 'Master the skills needed to lead teams and organizations', 44.99, 75, @BooksId, GETUTCDATE());

-- Home & Garden Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Stainless Steel Kitchen Knife Set', 'Professional-grade knife set with cutting board and storage case', 74.99, 90, @HomeGardenId, GETUTCDATE()),
    ('Garden Tool Set with Storage Bag', 'Complete 9-piece garden tool set with ergonomic handles', 49.99, 110, @HomeGardenId, GETUTCDATE()),
    ('Smart WiFi Doorbell Camera', 'Video doorbell with 1080p HD, night vision, and two-way audio', 99.99, 60, @HomeGardenId, GETUTCDATE()),
    ('Memory Foam Pillow (Set of 2)', 'Cooling gel memory foam pillows for better sleep', 39.99, 200, @HomeGardenId, GETUTCDATE()),
    ('LED Strip Lights 16.4ft', 'Color-changing LED lights with remote control and adhesive backing', 19.99, 300, @HomeGardenId, GETUTCDATE());

-- Sports Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Yoga Mat with Carrying Strap', 'Non-slip yoga mat with alignment lines and shoulder strap', 24.99, 250, @SportsId, GETUTCDATE()),
    ('Dumbbell Set (5-25lbs)', 'Complete set of hexagonal dumbbells for home gym training', 149.99, 50, @SportsId, GETUTCDATE()),
    ('Resistance Bands Set (5-piece)', 'Color-coded resistance bands with carrying bag and handles', 19.99, 300, @SportsId, GETUTCDATE()),
    ('Bicycle Helmet with LED Lights', 'Safety helmet with built-in LED lights and adjustable fit', 34.99, 120, @SportsId, GETUTCDATE()),
    ('Jump Rope Professional Grade', 'Speed rope with ball bearing handles for cardio training', 14.99, 400, @SportsId, GETUTCDATE());

-- Toys Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Building Block Set (1000 pieces)', 'Colorful building blocks compatible with major brands', 34.99, 180, @ToysId, GETUTCDATE()),
    ('Remote Control Drone with Camera', 'Easy-to-fly drone with 1080p camera and 18-minute flight time', 79.99, 65, @ToysId, GETUTCDATE()),
    ('Educational Science Kit', 'Complete STEM learning kit with 50+ experiments', 44.99, 95, @ToysId, GETUTCDATE()),
    ('Inflatable Water Slide', 'Giant inflatable water slide for outdoor summer fun', 89.99, 35, @ToysId, GETUTCDATE()),
    ('Board Game Collection (5 games)', 'Pack of 5 popular board games for family entertainment', 39.99, 110, @ToysId, GETUTCDATE());

-- Health & Beauty Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Organic Face Wash Kit', 'Natural organic face cleansers for all skin types (pack of 3)', 29.99, 200, @HealthBeautyId, GETUTCDATE()),
    ('Anti-Aging Night Cream', 'Premium moisturizer with retinol and peptides for youthful skin', 49.99, 120, @HealthBeautyId, GETUTCDATE()),
    ('Vitamin D Supplements (120 capsules)', 'High-potency vitamin D3 for immune and bone health', 14.99, 350, @HealthBeautyId, GETUTCDATE()),
    ('Facial Massage Roller (Jade Stone)', 'Natural jade stone roller for facial massage and puffiness reduction', 19.99, 180, @HealthBeautyId, GETUTCDATE()),
    ('Electric Toothbrush with UV Sterilizer', 'Sonic toothbrush with UV sanitizing charging case', 54.99, 85, @HealthBeautyId, GETUTCDATE());

-- Automotive Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryId, CreatedAt) VALUES
    ('Car Air Purifier with Hepa Filter', 'Portable air purifier for vehicle with USB power', 34.99, 150, @AutomotiveId, GETUTCDATE()),
    ('Phone Mount Dashboard', 'Adjustable phone holder for car dashboard with strong suction cup', 12.99, 400, @AutomotiveId, GETUTCDATE()),
    ('Tire Pressure Gauge Digital', 'Accurate digital tire gauge with backlit display', 9.99, 500, @AutomotiveId, GETUTCDATE()),
    ('Car Seat Cushion Memory Foam', 'Orthopedic car seat cushion for comfort and pain relief', 39.99, 180, @AutomotiveId, GETUTCDATE()),
    ('LED Headlight Bulbs (Pair)', 'Brightest LED conversion kits for better visibility', 44.99, 120, @AutomotiveId, GETUTCDATE());

SELECT 'Categories table created and 40 products inserted successfully' AS Status;
