namespace CommonArchitecture.Application.DTOs;

public class LogQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? StatusCode { get; set; }
    public string? Method { get; set; }
}
