using CommonArchitecture.Application.DTOs;

namespace CommonArchitecture.Web.Services;

public interface IOrderApiService
{
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusDto updateDto);
}
