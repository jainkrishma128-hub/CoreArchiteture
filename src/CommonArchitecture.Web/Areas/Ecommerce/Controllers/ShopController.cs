using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.Web.Areas.Ecommerce.Controllers;

[Area("Ecommerce")]
public class ShopController : Controller
{
    private readonly IProductApiService _productApiService;
    private readonly ILogger<ShopController> _logger;

    public ShopController(
        IProductApiService productApiService,
        ILogger<ShopController> logger)
    {
        _productApiService = productApiService;
        _logger = logger;
    }

    // GET: Ecommerce/Shop
    public IActionResult Index()
    {
        return View();
    }

    // GET: Ecommerce/Shop/Products - AJAX endpoint
    [HttpGet]
    public async Task<IActionResult> Products([FromQuery] ProductQueryParameters parameters)
    {
        try
        {
            parameters ??= new ProductQueryParameters();
            parameters.PageSize = parameters.PageSize > 0 ? parameters.PageSize : 12;
            
            var result = await _productApiService.GetAllAsync(parameters);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products for shop");
            return Json(new { success = false, message = "Error fetching products" });
        }
    }

    // GET: Ecommerce/Shop/ProductDetail/5
    public async Task<IActionResult> ProductDetail(int id)
    {
        try
        {
            var product = await _productApiService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", id);
            return NotFound();
        }
    }

    // GET: Ecommerce/Shop/Cart
    public IActionResult Cart()
    {
        return View();
    }
}

