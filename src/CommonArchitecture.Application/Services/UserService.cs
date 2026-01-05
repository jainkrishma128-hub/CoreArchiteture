using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<UserDto>> GetAllAsync(UserQueryParameters parameters)
    {
        var items = await _unitOfWork.Users.GetPagedAsync(parameters.SearchTerm, parameters.SortBy, parameters.SortOrder, parameters.PageNumber, parameters.PageSize);
        var total = await _unitOfWork.Users.GetTotalCountAsync(parameters.SearchTerm);
        var dtos = items.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Mobile = u.Mobile,
            RoleId = u.RoleId,
            RoleName = u.Role?.RoleName,
            ProfileImagePath = u.ProfileImagePath
        });
        return new PaginatedResult<UserDto> { Items = dtos, TotalCount = total, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize };
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var u = await _unitOfWork.Users.GetByIdAsync(id);
        if (u == null) return null;
        return new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Mobile = u.Mobile,
            RoleId = u.RoleId,
            RoleName = u.Role?.RoleName,
            ProfileImagePath = u.ProfileImagePath
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createDto, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Name = createDto.Name,
            Email = createDto.Email,
            Mobile = createDto.Mobile,
            RoleId = createDto.RoleId,
            ProfileImagePath = createDto.ProfileImagePath,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userWithRole = await _unitOfWork.Users.GetByIdAsync(created.Id);

        return new UserDto
        {
            Id = userWithRole!.Id,
            Name = userWithRole.Name,
            Email = userWithRole.Email,
            Mobile = userWithRole.Mobile,
            RoleId = userWithRole.RoleId,
            RoleName = userWithRole.Role?.RoleName,
            ProfileImagePath = userWithRole.ProfileImagePath
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto updateDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;
        user.Name = updateDto.Name;
        user.Email = updateDto.Email;
        user.Mobile = updateDto.Mobile;
        user.RoleId = updateDto.RoleId;
        user.ProfileImagePath = updateDto.ProfileImagePath;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;
        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

