using ToyShoppingWebApp.Domain.Entities;

namespace ToyShoppingWebApp.Application.Repositories.Interfaces
{
    public interface IToyRepository
    {
        /// <summary>
        /// Get toys by category with pagination
        /// </summary>
        Task<(List<Toy>, int totalCount)> GetToysByCategoryAsync(int categoryId, int page, int pageSize);

        /// <summary>
        /// Get all active toys
        /// </summary>
        Task<List<Toy>> GetAllToysAsync();

        /// <summary>
        /// Get toy by ID
        /// </summary>
        Task<Toy?> GetToyByIdAsync(int id);

        /// <summary>
        /// Create new toy
        /// </summary>
        Task<Toy> CreateToyAsync(Toy toy);

        /// <summary>
        /// Update existing toy
        /// </summary>
        Task<Toy> UpdateToyAsync(Toy toy);

        /// <summary>
        /// Delete toy (soft delete)
        /// </summary>
        Task DeleteToyAsync(int id);
    }
}