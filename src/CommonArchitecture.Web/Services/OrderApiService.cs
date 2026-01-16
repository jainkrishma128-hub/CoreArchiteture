using CommonArchitecture.Application.DTOs;
using System.Net.Http.Json;

namespace CommonArchitecture.Web.Services;

public class OrderApiService : IOrderApiService
{
    private readonly HttpClient _httpClient;

    public OrderApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OrderDto>($"api/Orders/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<OrderDto>>("api/Orders") ?? new List<OrderDto>();
        }
        catch
        {
            return new List<OrderDto>();
        }
    }

    public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Orders", createOrderDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusDto updateDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Orders/{id}/status", updateDto);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
