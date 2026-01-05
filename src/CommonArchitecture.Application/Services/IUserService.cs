using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Application.Services;

public interface IUserService
{
 Task<PaginatedResult<UserDto>> GetAllAsync(UserQueryParameters parameters);
 Task<UserDto?> GetByIdAsync(int id);
 Task<UserDto> CreateAsync(CreateUserDto createDto, CancellationToken cancellationToken = default);
 Task<bool> UpdateAsync(int id, UpdateUserDto updateDto, CancellationToken cancellationToken = default);
 Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
