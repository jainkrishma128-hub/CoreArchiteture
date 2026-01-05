namespace CommonArchitecture.Core.Entities;

public class RoleMenu
{
 public int Id { get; set; }
 public int RoleId { get; set; }
 public int MenuId { get; set; }
 public bool CanCreate { get; set; }
 public bool CanRead { get; set; }
 public bool CanUpdate { get; set; }
 public bool CanDelete { get; set; }
 public bool CanExecute { get; set; } // For special actions/exports
 public DateTime CreatedAt { get; set; }
 public DateTime? UpdatedAt { get; set; }

 // Navigation properties
 public Role? Role { get; set; }
 public Menu? Menu { get; set; }
}
