using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<InventoryDto>>> GetSummary([FromQuery] InventoryQueryParameters parameters)
    {
        var result = await _inventoryService.GetInventorySummaryAsync(parameters);
        return Ok(result);
    }

    [HttpGet("{productId}/transactions")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactions(int productId)
    {
        var result = await _inventoryService.GetProductTransactionsAsync(productId);
        return Ok(result);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock(StockAdjustmentDto adjustmentDto)
    {
        var userId = User.Identity?.Name;
        var result = await _inventoryService.AdjustStockAsync(adjustmentDto, userId);
        if (!result) return BadRequest("Failed to adjust stock. Product not found.");
        return Ok();
    }
}
