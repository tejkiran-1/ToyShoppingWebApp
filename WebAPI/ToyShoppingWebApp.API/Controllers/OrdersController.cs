namespace ToyShoppingWebApp.API.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Services.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All order endpoints require authentication
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Place new order (authenticated users)
        /// </summary>
        [HttpPost]
        [Produces(typeof(OrderDto))]
        [Consumes("application/json")]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // Get user ID from JWT claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var order = await _orderService.PlaceOrderAsync(userId, request);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's orders with pagination
        /// </summary>
        [HttpGet("my-orders")]
        [Produces(typeof(PaginatedResponse<OrderDto>))]
        public async Task<ActionResult<PaginatedResponse<OrderDto>>> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get user ID from JWT claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get order by ID (user can only see their own, admin can see all)
        /// </summary>
        [HttpGet("{id}")]
        [Produces(typeof(OrderDto))]
        public async Task<ActionResult<OrderDto>> GetOrderById([FromRoute] int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                    return NotFound(new { error = $"Order with id {id} not found" });

                // Check authorization: user can only see their own orders (unless admin)
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (order.UserId != userId && userRole != "Admin")
                    return Forbid();

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all orders (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Produces(typeof(PaginatedResponse<OrderDto>))]
        public async Task<ActionResult<PaginatedResponse<OrderDto>>> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _orderService.GetAllOrdersAsync(page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}