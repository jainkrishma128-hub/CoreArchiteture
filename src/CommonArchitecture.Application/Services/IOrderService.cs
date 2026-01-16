using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Enums;

namespace CommonArchitecture.Application.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status);
}
