using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResult<MenuDto>>> GetAll([FromQuery] MenuQueryParameters parameters)
    {
        var result = await _menuService.GetAllAsync(parameters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuDto>> GetById(int id)
    {
        var menu = await _menuService.GetByIdAsync(id);
        
        if (menu == null)
            return NotFound();

        return Ok(menu);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MenuDto>> Create(CreateMenuDto createDto)
    {
        var menu = await _menuService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = menu.Id }, menu);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateMenuDto updateDto)
    {
        var result = await _menuService.UpdateAsync(id, updateDto);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _menuService.DeleteAsync(id);
        
        if (!result)
            return NotFound();

        return NoContent();
    }
}
