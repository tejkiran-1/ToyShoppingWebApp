namespace ToyShoppingWebApp.Application.Services.Interfaces
{
    using ToyShoppingWebApp.Application.DTOs;

    public interface IToyService
    {
        /// <summary>
        /// Get toys by category with pagination
        /// </summary>
        Task<PaginatedResponse<ToyDto>> GetToysByCategoryAsync(
            int categoryId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get all active toys
        /// </summary>
        Task<List<ToyDto>> GetAllToysAsync();

        /// <summary>
        /// Get toy by ID
        /// </summary>
        Task<ToyDto?> GetToyByIdAsync(int id);

        /// <summary>
        /// Create toy (admin only)
        /// </summary>
        Task<ToyDto> CreateToyAsync(CreateToyRequest request);

        /// <summary>
        /// Update toy (admin only)
        /// </summary>
        Task<ToyDto> UpdateToyAsync(int id, CreateToyRequest request);

        /// <summary>
        /// Delete toy (admin only)
        /// </summary>
        Task DeleteToyAsync(int id);
    }
}