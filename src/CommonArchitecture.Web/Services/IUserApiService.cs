using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IUserApiService
{
    Task<PaginatedResult<UserDto>> GetAllAsync(UserQueryParameters parameters);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto createDto, IFormFile? profileImage);
    Task<bool> UpdateAsync(int id, UpdateUserDto updateDto, IFormFile? profileImage);
    Task<bool> DeleteAsync(int id);
}

