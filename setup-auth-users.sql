-- Authentication System Setup Script
-- This script creates test users for the authentication system

-- First, ensure roles exist
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 1)
BEGIN
    INSERT INTO Roles (RoleName, CreatedAt)
    VALUES ('Admin', GETDATE());
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 2)
BEGIN
    INSERT INTO Roles (RoleName, CreatedAt)
    VALUES ('Product Manager', GETDATE());
END

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

-- Display all users with their roles
SELECT 
    u.Id,
    u.Name,
    u.Email,
    u.Mobile,
    r.RoleName,
    u.CreatedAt
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.Id
ORDER BY u.RoleId, u.Name;

PRINT '';
PRINT '=== Test Credentials ===';
PRINT 'Admin Login:';
PRINT '  Mobile: 9876543210';
PRINT '  OTP: 1234';
PRINT '';
PRINT 'Product Manager Login:';
PRINT '  Mobile: 9876543211';
PRINT '  OTP: 1234';
