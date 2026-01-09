CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256) NOT NULL,
    Description NVARCHAR(1000),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2
);

ALTER TABLE Products ADD CategoryId INT NULL;
ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE;
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);

-- Insert default categories
INSERT INTO Categories (Name, Description, IsActive) VALUES
    ('Electronics', 'Electronic devices and gadgets', 1),
    ('Clothing', 'Apparel and fashion items', 1),
    ('Books', 'Books and educational materials', 1),
    ('Home & Garden', 'Home improvement and gardening supplies', 1),
    ('Sports', 'Sports equipment and accessories', 1),
    ('Toys', 'Toys and games for all ages', 1),
    ('Health & Beauty', 'Health and beauty products', 1),
    ('Automotive', 'Car parts and automotive accessories', 1);

SELECT 'Categories table created and seeded' AS Status;
