using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleMenusController : ControllerBase
{
    private readonly IRoleMenuService _roleMenuService;
    private readonly ILogger<RoleMenusController> _logger;

    public RoleMenusController(IRoleMenuService roleMenuService, ILogger<RoleMenusController> logger)
    {
        _roleMenuService = roleMenuService;
        _logger = logger;
    }

    [HttpGet("role/{roleId}")]
    public async Task<ActionResult<RoleMenuPermissionsDto>> GetByRole(int roleId)
    {
        try
        {
            var result = await _roleMenuService.GetRoleMenuPermissionsAsync(roleId);
            
            if (result == null)
                return NotFound(new { message = "Role not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role menu permissions for role {RoleId}", roleId);
            return StatusCode(500, new { message = "An error occurred while fetching role menu permissions" });
        }
    }

    [HttpPut("role/{roleId}")]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] List<RoleMenuItemDto> menuPermissions)
    {
        try
        {
            if (menuPermissions == null || menuPermissions.Count == 0)
            {
                return BadRequest(new { message = "Menu permissions cannot be empty" });
            }

            var result = await _roleMenuService.UpdateRoleMenuPermissionsAsync(roleId, menuPermissions);

            if (!result)
            {
                return NotFound(new { message = "Role not found" });
            }

            return Ok(new { message = "Role menu permissions updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role menu permissions for role {RoleId}", roleId);
            return StatusCode(500, new { message = "An error occurred while updating role menu permissions" });
        }
    }
}
