using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.Web.ViewComponents;

public class CategoryViewComponent : ViewComponent
{
    private readonly ICategoryApiService _categoryApiService;

    public CategoryViewComponent(ICategoryApiService categoryApiService)
    {
        _categoryApiService = categoryApiService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var result = await _categoryApiService.GetActiveAsync();
        return View(result);
    }
}
