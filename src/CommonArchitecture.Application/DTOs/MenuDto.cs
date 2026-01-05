namespace CommonArchitecture.Application.DTOs;

public class MenuDto
{
 public int Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public string Icon { get; set; } = string.Empty;
 public string Url { get; set; } = string.Empty;
 public int? ParentMenuId { get; set; }
 public int DisplayOrder { get; set; }
 public bool IsActive { get; set; }
}

public class CreateMenuDto
{
 public string Name { get; set; } = string.Empty;
 public string Icon { get; set; } = string.Empty;
 public string Url { get; set; } = string.Empty;
 public int? ParentMenuId { get; set; }
 public int DisplayOrder { get; set; }
}

public class UpdateMenuDto
{
 public string Name { get; set; } = string.Empty;
 public string Icon { get; set; } = string.Empty;
 public string Url { get; set; } = string.Empty;
 public int? ParentMenuId { get; set; }
 public int DisplayOrder { get; set; }
 public bool IsActive { get; set; }
}

public class MenuQueryParameters
{
 public int PageNumber { get; set; } = 1;
 public int PageSize { get; set; } = 10;
 public string SortBy { get; set; } = "DisplayOrder";
 public string SortOrder { get; set; } = "asc";
 public string? SearchTerm { get; set; }
}
