namespace ToyShoppingWebApp.Application.Repositories.Interfaces
{
    using ToyShoppingWebApp.Domain.Entities;

    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
    }
}