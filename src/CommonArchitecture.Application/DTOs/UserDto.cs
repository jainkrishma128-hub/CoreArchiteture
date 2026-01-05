namespace CommonArchitecture.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? ProfileImagePath { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? ProfileImagePath { get; set; }
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? ProfileImagePath { get; set; }
}

public class UserQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "Id";
    public string SortOrder { get; set; } = "asc"; // asc or desc
    public int PageNumber { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

