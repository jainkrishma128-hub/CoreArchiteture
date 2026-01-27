namespace CommonArchitecture.Application.DTOs;

public class CategoryProductGroupDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
}
