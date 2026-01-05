namespace CommonArchitecture.Core.Entities;

public class Menu
{
 public int Id { get; set; }
 public string Name { get; set; } = string.Empty;
 public string Icon { get; set; } = string.Empty;
 public string Url { get; set; } = string.Empty;
 public int? ParentMenuId { get; set; }
 public int DisplayOrder { get; set; }
 public bool IsActive { get; set; } = true;
 public DateTime CreatedAt { get; set; }
 public DateTime? UpdatedAt { get; set; }

 public Menu? ParentMenu { get; set; }
 public ICollection<Menu>? SubMenus { get; set; }
}
