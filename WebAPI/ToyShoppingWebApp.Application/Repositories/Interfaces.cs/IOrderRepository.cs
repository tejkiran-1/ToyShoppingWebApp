using ToyShoppingWebApp.Domain.Entities;

namespace ToyShoppingWebApp.Application.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        /// <summary>
        /// Get orders for a specific user
        /// </summary>
        Task<(List<Order>, int totalCount)> GetUserOrdersAsync(int userId, int page, int pageSize);

        /// <summary>
        /// Get all orders (admin only)
        /// </summary>
        Task<(List<Order>, int totalCount)> GetAllOrdersAsync(int page, int pageSize);

        /// <summary>
        /// Get order by ID
        /// </summary>
        Task<Order?> GetOrderByIdAsync(int id);

        /// <summary>
        /// Create new order
        /// </summary>
        Task<Order> CreateOrderAsync(Order order);

        /// <summary>
        /// Update order status
        /// </summary>
        Task<Order> UpdateOrderAsync(Order order);
    }
}