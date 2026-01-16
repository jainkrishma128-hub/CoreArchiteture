using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Enums;
using CommonArchitecture.Core.Interfaces;

namespace CommonArchitecture.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        var lastOrderNumber = await _orderRepository.GetLastOrderNumberAsync();
        var nextNumber = 1;

        if (!string.IsNullOrEmpty(lastOrderNumber) && lastOrderNumber.StartsWith("ORD-"))
        {
            if (int.TryParse(lastOrderNumber.Substring(4), out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        var order = new Order
        {
            OrderNumber = $"ORD-{nextNumber:D6}",
            CustomerName = createOrderDto.CustomerName,
            Email = createOrderDto.Email,
            Phone = createOrderDto.Phone,
            Address = createOrderDto.Address,
            City = createOrderDto.City,
            ZipCode = createOrderDto.ZipCode,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow
        };

        decimal subtotal = 0;

        foreach (var itemDto in createOrderDto.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product != null)
            {
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = itemDto.Quantity,
                    TotalPrice = product.Price * itemDto.Quantity
                };
                order.OrderItems.Add(orderItem);
                subtotal += orderItem.TotalPrice;
            }
        }

        order.Subtotal = subtotal;
        order.Tax = subtotal * 0.10m; // 10% tax for example
        order.TotalAmount = order.Subtotal + order.Tax;

        var createdOrder = await _orderRepository.AddAsync(order);
        return MapToDto(createdOrder);
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return false;

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
        return true;
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            CustomerName = order.CustomerName,
            Email = order.Email,
            Phone = order.Phone,
            Address = order.Address,
            City = order.City,
            ZipCode = order.ZipCode,
            Subtotal = order.Subtotal,
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}
