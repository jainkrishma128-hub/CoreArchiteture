using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IWebHostEnvironment environment, ILogger<UsersController> logger)
    {
        _userService = userService;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<UserDto>>> GetAll([FromQuery] UserQueryParameters parameters)
    {
        var result = await _userService.GetAllAsync(parameters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromForm] CreateUserFormDto formDto, CancellationToken cancellationToken)
    {
        string? imagePath = null;
        
        // Handle file upload
        if (formDto.ProfileImage != null && formDto.ProfileImage.Length > 0)
        {
            imagePath = await SaveProfileImage(formDto.ProfileImage);
        }

        var createDto = new CreateUserDto
        {
            Name = formDto.Name,
            Email = formDto.Email,
            Mobile = formDto.Mobile,
            RoleId = formDto.RoleId,
            ProfileImagePath = imagePath
        };

        var user = await _userService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateUserFormDto formDto, CancellationToken cancellationToken)
    {
        string? imagePath = formDto.ExistingProfileImagePath;

        // Handle file upload - if new image provided, replace old one
        if (formDto.ProfileImage != null && formDto.ProfileImage.Length > 0)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(imagePath))
            {
                DeleteProfileImage(imagePath);
            }
            imagePath = await SaveProfileImage(formDto.ProfileImage);
        }

        var updateDto = new UpdateUserDto
        {
            Name = formDto.Name,
            Email = formDto.Email,
            Mobile = formDto.Mobile,
            RoleId = formDto.RoleId,
            ProfileImagePath = imagePath
        };

        var result = await _userService.UpdateAsync(id, updateDto, cancellationToken);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        if (!string.IsNullOrEmpty(user.ProfileImagePath))
        {
            DeleteProfileImage(user.ProfileImagePath);
        }

        var result = await _userService.DeleteAsync(id, cancellationToken);
        if (!result)
            return NotFound();

        return NoContent();
    }

    private async Task<string> SaveProfileImage(IFormFile file)
    {
        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Invalid file type. Only image files are allowed.");
        }

        // Validate file size (max 5MB)
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            throw new ArgumentException("File size exceeds the maximum allowed size of 5MB.");
        }

        // Create wwwroot/uploads/profiles directory if it doesn't exist
        var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
        }
        
        var uploadsFolder = Path.Combine(wwwrootPath, "uploads", "profiles");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate unique file name
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Return relative path for storage in database
        return Path.Combine("uploads", "profiles", uniqueFileName).Replace("\\", "/");
    }

    private void DeleteProfileImage(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(wwwrootPath, imagePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile image: {ImagePath}", imagePath);
            // Don't throw - image deletion failure shouldn't prevent user deletion
        }
    }
}

// DTOs for form data binding
public class CreateUserFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

public class UpdateUserFormDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public string? ExistingProfileImagePath { get; set; }
}

