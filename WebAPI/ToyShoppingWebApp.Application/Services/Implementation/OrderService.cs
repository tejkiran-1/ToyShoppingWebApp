namespace ToyShoppingWebApp.Application.Services.Implementations
{
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Application.Services.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;
    using Microsoft.AspNetCore.Http;

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IToyRepository _toyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(
            IOrderRepository orderRepository,
            IToyRepository toyRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _toyRepository = toyRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderDto> PlaceOrderAsync(int userId, CreateOrderRequest request)
        {
            // Validate input
            if (request.Items == null || request.Items.Count == 0)
                throw new ArgumentException("Order must contain at least one item");

            // Get current user ID from JWT
            var currentUserId = int.Parse(
                _httpContextAccessor.HttpContext!.User
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Authorization check: User can only create order for themselves
            // (unless they're an admin, but we enforce at controller level)
            if (currentUserId != userId)
                throw new UnauthorizedAccessException("You cannot create orders for other users");

            // Validate all toys exist and have stock
            var orderItems = new List<OrderItem>();
            decimal totalPrice = 0;

            foreach (var item in request.Items)
            {
                // Get toy
                var toy = await _toyRepository.GetToyByIdAsync(item.ToyId);
                if (toy == null)
                    throw new KeyNotFoundException($"Toy with id {item.ToyId} not found");

                // Check stock
                if (toy.Stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {toy.Name}. Available: {toy.Stock}, Requested: {item.Quantity}");

                // Reduce stock
                toy.Stock -= item.Quantity;
                await _toyRepository.UpdateToyAsync(toy);

                // Create order item
                var orderItem = new OrderItem
                {
                    ToyId = item.ToyId,
                    Quantity = item.Quantity,
                    UnitPrice = toy.Price
                };
                orderItems.Add(orderItem);

                // Add to total
                totalPrice += toy.Price * item.Quantity;
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                OrderItems = orderItems
            };

            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // Map to DTO and return
            return MapOrderToDto(createdOrder);
        }

        public async Task<PaginatedResponse<OrderDto>> GetUserOrdersAsync(int userId, int page = 1, int pageSize = 10)
        {
            // Validate pagination
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and pageSize must be greater than 0");

            if (pageSize > 100)
                pageSize = 100;

            // Get orders from repository
            var (orders, totalCount) = await _orderRepository
                .GetUserOrdersAsync(userId, page, pageSize);

            // Map to DTOs
            var orderDtos = orders.Select(MapOrderToDto).ToList();

            return new PaginatedResponse<OrderDto>
            {
                Data = orderDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<PaginatedResponse<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10)
        {
            // Validate pagination
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and pageSize must be greater than 0");

            if (pageSize > 100)
                pageSize = 100;

            // Get all orders from repository
            var (orders, totalCount) = await _orderRepository
                .GetAllOrdersAsync(page, pageSize);

            // Map to DTOs
            var orderDtos = orders.Select(MapOrderToDto).ToList();

            return new PaginatedResponse<OrderDto>
            {
                Data = orderDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null)
                return null;

            return MapOrderToDto(order);
        }

        /// <summary>
        /// Helper method to map Order entity to OrderDto
        /// </summary>
        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ToyId = oi.ToyId,
                    ToyName = oi.Toy?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}