namespace ToyShoppingWebApp.Application.Services.Interfaces
{
    using ToyShoppingWebApp.Application.DTOs;

    public interface ICategoryService
    {
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> UpdateCategoryAsync(int id, CreateCategoryRequest request);
        Task DeleteCategoryAsync(int id);
    }
}