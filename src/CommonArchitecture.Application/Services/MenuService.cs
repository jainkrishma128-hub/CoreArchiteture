using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.Application.Services;

public class MenuService : IMenuService
{
    private readonly IUnitOfWork _unitOfWork;

    public MenuService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<MenuDto>> GetAllAsync(MenuQueryParameters parameters)
    {
        var items = await _unitOfWork.Menus.GetPagedAsync(parameters.SearchTerm, parameters.SortBy, parameters.SortOrder, parameters.PageNumber, parameters.PageSize);
        var total = await _unitOfWork.Menus.GetTotalCountAsync(parameters.SearchTerm);

        var dtos = items.Select(m => new MenuDto
        {
            Id = m.Id,
            Name = m.Name,
            Icon = m.Icon,
            Url = m.Url,
            ParentMenuId = m.ParentMenuId,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive
        });

        return new PaginatedResult<MenuDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<MenuDto?> GetByIdAsync(int id)
    {
        var menu = await _unitOfWork.Menus.GetByIdAsync(id);
        if (menu == null) return null;

        return new MenuDto
        {
            Id = menu.Id,
            Name = menu.Name,
            Icon = menu.Icon,
            Url = menu.Url,
            ParentMenuId = menu.ParentMenuId,
            DisplayOrder = menu.DisplayOrder,
            IsActive = menu.IsActive
        };
    }

    public async Task<MenuDto> CreateAsync(CreateMenuDto createDto, CancellationToken cancellationToken = default)
    {
        var menu = new Menu
        {
            Name = createDto.Name,
            Icon = createDto.Icon,
            Url = createDto.Url,
            ParentMenuId = createDto.ParentMenuId,
            DisplayOrder = createDto.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Menus.AddAsync(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MenuDto
        {
            Id = menu.Id,
            Name = menu.Name,
            Icon = menu.Icon,
            Url = menu.Url,
            ParentMenuId = menu.ParentMenuId,
            DisplayOrder = menu.DisplayOrder,
            IsActive = menu.IsActive
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateMenuDto updateDto, CancellationToken cancellationToken = default)
    {
        var menu = await _unitOfWork.Menus.GetByIdAsync(id);
        if (menu == null) return false;

        menu.Name = updateDto.Name;
        menu.Icon = updateDto.Icon;
        menu.Url = updateDto.Url;
        menu.ParentMenuId = updateDto.ParentMenuId;
        menu.DisplayOrder = updateDto.DisplayOrder;
        menu.IsActive = updateDto.IsActive;
        menu.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Menus.UpdateAsync(menu);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var menu = await _unitOfWork.Menus.GetByIdAsync(id);
        if (menu == null) return false;

        await _unitOfWork.Menus.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
