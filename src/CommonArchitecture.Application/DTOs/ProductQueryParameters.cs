namespace CommonArchitecture.Application.DTOs;

public class ProductQueryParameters
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
