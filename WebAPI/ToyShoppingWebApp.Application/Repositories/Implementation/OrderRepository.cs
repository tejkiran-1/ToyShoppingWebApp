namespace ToyShoppingWebApp.Application.Repositories.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using ToyShoppingWebApp.Application.Data;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class OrderRepository : IOrderRepository
    {
        private readonly ToyShoppingDbContext _context;

        public OrderRepository(ToyShoppingDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Order>, int)> GetUserOrdersAsync(
            int userId, int page, int pageSize)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toy);

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<(List<Order>, int)> GetAllOrdersAsync(
            int page, int pageSize)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toy);

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toy)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}