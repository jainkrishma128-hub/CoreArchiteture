namespace CommonArchitecture.Application.DTOs;

public class RoleMenuDto
{
 public int Id { get; set; }
 public int RoleId { get; set; }
 public int MenuId { get; set; }
 public string MenuName { get; set; } = string.Empty;
 public bool CanCreate { get; set; }
 public bool CanRead { get; set; }
 public bool CanUpdate { get; set; }
 public bool CanDelete { get; set; }
 public bool CanExecute { get; set; }
}

public class CreateRoleMenuDto
{
 public int RoleId { get; set; }
 public int MenuId { get; set; }
 public bool CanCreate { get; set; }
 public bool CanRead { get; set; }
 public bool CanUpdate { get; set; }
 public bool CanDelete { get; set; }
 public bool CanExecute { get; set; }
}

public class UpdateRoleMenuDto
{
 public bool CanCreate { get; set; }
 public bool CanRead { get; set; }
 public bool CanUpdate { get; set; }
 public bool CanDelete { get; set; }
 public bool CanExecute { get; set; }
}

public class RoleMenuPermissionsDto
{
 public int RoleId { get; set; }
 public string RoleName { get; set; } = string.Empty;
 public List<RoleMenuItemDto> MenuPermissions { get; set; } = new();
}

public class RoleMenuItemDto
{
 public int MenuId { get; set; }
 public string MenuName { get; set; } = string.Empty;
 public bool CanCreate { get; set; }
 public bool CanRead { get; set; }
 public bool CanUpdate { get; set; }
 public bool CanDelete { get; set; }
 public bool CanExecute { get; set; }
}

public class RoleMenuQueryParameters
{
 public int PageNumber { get; set; } = 1;
 public int PageSize { get; set; } = 10;
 public string SortBy { get; set; } = "Id";
 public string SortOrder { get; set; } = "asc";
 public int? RoleId { get; set; }
}
