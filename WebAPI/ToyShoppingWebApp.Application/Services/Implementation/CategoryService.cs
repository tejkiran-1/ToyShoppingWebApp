namespace ToyShoppingWebApp.Application.Services.Implementations
{
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Application.Services.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required");

            // Create entity
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            };

            // Save
            var created = await _categoryRepository.CreateCategoryAsync(category);

            // Return DTO
            return new CategoryDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description
            };
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
                return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, CreateCategoryRequest request)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
                throw new KeyNotFoundException($"Category with id {id} not found");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required");

            category.Name = request.Name;
            category.Description = request.Description;

            var updated = await _categoryRepository.UpdateCategoryAsync(category);

            return new CategoryDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description
            };
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);

            if (category == null)
                throw new KeyNotFoundException($"Category with id {id} not found");

            await _categoryRepository.DeleteCategoryAsync(id);
        }
    }
}