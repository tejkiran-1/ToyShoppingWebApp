namespace ToyShoppingWebApp.Application.Repositories.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using ToyShoppingWebApp.Application.Data;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class UserRepository : IUserRepository
    {
        private readonly ToyShoppingDbContext _context;

        public UserRepository(ToyShoppingDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.IsActive);
        }
    }
}