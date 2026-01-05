using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<RoleDto>> GetAllAsync(RoleQueryParameters parameters)
    {
        var items = await _unitOfWork.Roles.GetPagedAsync(parameters.SearchTerm, parameters.SortBy, parameters.SortOrder, parameters.PageNumber, parameters.PageSize);
        var total = await _unitOfWork.Roles.GetTotalCountAsync(parameters.SearchTerm);
        var dtos = items.Select(r => new RoleDto { Id = r.Id, RoleName = r.RoleName });
        return new PaginatedResult<RoleDto> { Items = dtos, TotalCount = total, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize };
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null) return null;
        return new RoleDto { Id = role.Id, RoleName = role.RoleName };
    }

    public async Task<RoleDto> CreateAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = new Role { RoleName = roleName, CreatedAt = DateTime.UtcNow };
        var created = await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new RoleDto { Id = created.Id, RoleName = created.RoleName };
    }

    public async Task<bool> UpdateAsync(int id, string roleName, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null) return false;
        role.RoleName = roleName;
        role.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null) return false;
        await _unitOfWork.Roles.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

