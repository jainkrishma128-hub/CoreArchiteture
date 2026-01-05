using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAll([FromQuery] ProductQueryParameters parameters)
    {
        var result = await _productService.GetAllAsync(parameters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto createDto, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateProductDto updateDto, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, updateDto, cancellationToken);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("export")]
    [Authorize]
    public async Task<IActionResult> Export()
    {
        var fileContent = await _productService.ExportAsync();
        return File(fileContent, "text/csv", $"products_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
    }

    [HttpPost("import")]
    [Authorize]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        using var stream = file.OpenReadStream();
        var result = await _productService.ImportAsync(stream);

        if (!result)
            return BadRequest("Failed to import products. Ensure the file is a valid CSV.");

        return Ok(new { Count = "Batch Processed" });
    }
}
