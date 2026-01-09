using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{ 

    private readonly ILogger<CategoriesController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(ILogger<CategoriesController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all active categories (public endpoint for shop)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetQuery()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active categories");
            return StatusCode(500, new { message = "Error fetching active categories" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetAll([FromQuery] CategoryQueryParameters parameters)
    {
        try
        {
            var query = _unitOfWork.Categories.GetQuery();

            // Search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
 {
   query = query.Where(c => c.Name.Contains(parameters.SearchTerm) || c.Description.Contains(parameters.SearchTerm));
 }

    // Sort - use simple OrderBy/OrderByDescending for dynamic sorting
            if (parameters.SortOrder?.ToLower() == "desc")
       {
                query = parameters.SortBy?.ToLower() == "name" 
   ? query.OrderByDescending(c => c.Name)
              : parameters.SortBy?.ToLower() == "description"
          ? query.OrderByDescending(c => c.Description)
     : query.OrderByDescending(c => c.Id);
            }
      else
            {
   query = parameters.SortBy?.ToLower() == "name"
  ? query.OrderBy(c => c.Name)
       : parameters.SortBy?.ToLower() == "description"
     ? query.OrderBy(c => c.Description)
          : query.OrderBy(c => c.Id);
            }

          // Paginate - CountAsync and ToListAsync for async execution
    var totalCount = await query.CountAsync();
           var items = await query
 .Skip((parameters.PageNumber - 1) * parameters.PageSize)
              .Take(parameters.PageSize)
      .Select(c => new CategoryDto
     {
      Id = c.Id,
            Name = c.Name,
     Description = c.Description,
        IsActive = c.IsActive
         })
   .ToListAsync();

      return Ok(new PaginatedResult<CategoryDto>
          {
         Items = items,
   TotalCount = totalCount,
    PageNumber = parameters.PageNumber,
   PageSize = parameters.PageSize
         });
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Error fetching categories");
           return StatusCode(500, new { message = "Error fetching categories" });
        }
    }

    [HttpGet("{id}")]
public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        try
        {
      var category = await _unitOfWork.Categories.GetByIdAsync(id);
       if (category == null)
  {
       return NotFound(new { message = "Category not found" });
     }

        return Ok(new CategoryDto
         {
     Id = category.Id,
         Name = category.Name,
          Description = category.Description,
         IsActive = category.IsActive
            });
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Error fetching category {CategoryId}", id);
           return StatusCode(500, new { message = "Error fetching category" });
        }
    }

   [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto createDto)
    {
        try
     {
            if (!ModelState.IsValid)
      {
return BadRequest(ModelState);
 }

    var category = new Category
         {
 Name = createDto.Name,
         Description = createDto.Description,
              IsActive = createDto.IsActive,
     CreatedAt = DateTime.UtcNow
         };

   _unitOfWork.Categories.Add(category);
  await _unitOfWork.SaveChangesAsync();

         return CreatedAtAction(nameof(GetById), new { id = category.Id }, new CategoryDto
{
     Id = category.Id,
  Name = category.Name,
                Description = category.Description,
     IsActive = category.IsActive
   });
     }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "Error creating category" });
        }
    }

   [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateDto)
    {
      try
  {
    if (!ModelState.IsValid)
            {
 return BadRequest(ModelState);
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
    return NotFound(new { message = "Category not found" });
     }

      category.Name = updateDto.Name;
    category.Description = updateDto.Description;
            category.IsActive = updateDto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

     _unitOfWork.Categories.Update(category);
     await _unitOfWork.SaveChangesAsync();

          return Ok(new { message = "Category updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, new { message = "Error updating category" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
  try
        {
          var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            {
       return NotFound(new { message = "Category not found" });
            }

       _unitOfWork.Categories.Delete(category);
       await _unitOfWork.SaveChangesAsync();

         return Ok(new { message = "Category deleted successfully" });
        }
      catch (Exception ex)
        {
          _logger.LogError(ex, "Error deleting category {CategoryId}", id);
         return StatusCode(500, new { message = "Error deleting category" });
      }
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        try
        {
            var categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();
   
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Id,Name,Description,IsActive,CreatedAt");

       foreach (var category in categories)
       {
    csvBuilder.AppendLine($"\"{category.Id}\",\"{category.Name}\",\"{category.Description}\",\"{category.IsActive}\",\"{category.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
     }

            var fileContent = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(fileContent, "text/csv", $"categories_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Error exporting categories");
       return StatusCode(500, new { message = "Error exporting categories" });
        }
    }

    [HttpPost("import")]
public async Task<IActionResult> Import(IFormFile file)
    {
  try
        {
     if (file == null || file.Length == 0)
     {
         return BadRequest(new { message = "Please select a file" });
 }

       using var reader = new StreamReader(file.OpenReadStream());
 var csv = await reader.ReadToEndAsync();
       var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
        {
                return BadRequest(new { message = "CSV file must contain at least one data row" });
            }

            var importedCount = 0;
            for (int i = 1; i < lines.Length; i++)
     {
  var fields = lines[i].Split(',');
      if (fields.Length < 3)
      continue;

     var category = new Category
        {
          Name = fields[1].Trim().Trim('"'),
              Description = fields[2].Trim().Trim('"'),
           IsActive = fields.Length > 3 && bool.Parse(fields[3]),
 CreatedAt = DateTime.UtcNow
      };

    _unitOfWork.Categories.Add(category);
  importedCount++;
       }

        await _unitOfWork.SaveChangesAsync();
      return Ok(new { message = $"Imported {importedCount} categories successfully" });
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error importing categories");
return StatusCode(500, new { message = "Error importing categories" });
        }
    }
}
