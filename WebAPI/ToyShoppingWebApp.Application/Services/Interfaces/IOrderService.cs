namespace ToyShoppingWebApp.Application.Services.Interfaces
{
    using ToyShoppingWebApp.Application.DTOs;

    public interface IOrderService
    {
        /// <summary>
        /// Place new order (customer)
        /// </summary>
        Task<OrderDto> PlaceOrderAsync(int userId, CreateOrderRequest request);

        /// <summary>
        /// Get orders for specific user (customer can only see their own)
        /// </summary>
        Task<PaginatedResponse<OrderDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get all orders (admin only)
        /// </summary>
        Task<PaginatedResponse<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Get order by ID
        /// </summary>
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
    }
}