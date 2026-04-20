using ToyShoppingWebApp.Domain.Entities;

namespace ToyShoppingWebApp.Application.Repositories.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<User?> GetUserByIdAsync(int id);

        /// <summary>
        /// Get user by email (for login)
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Create new user
        /// </summary>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// Check if user exists by email
        /// </summary>
        Task<bool> UserExistsByEmailAsync(string email);
    }
}