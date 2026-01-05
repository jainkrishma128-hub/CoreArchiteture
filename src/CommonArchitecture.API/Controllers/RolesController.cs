using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResult<RoleDto>>> GetAll([FromQuery] RoleQueryParameters parameters)
    {
        var result = await _roleService.GetAllAsync(parameters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RoleDto>> Create(CreateRoleDto createDto, CancellationToken cancellationToken)
    {
        var role = await _roleService.CreateAsync(createDto.RoleName, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateRoleDto updateDto, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, updateDto.RoleName, cancellationToken);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

