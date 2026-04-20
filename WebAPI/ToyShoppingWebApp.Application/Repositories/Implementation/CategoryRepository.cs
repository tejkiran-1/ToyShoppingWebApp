namespace ToyShoppingWebApp.Application.Repositories.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using ToyShoppingWebApp.Application.Data;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class CategoryRepository : ICategoryRepository
    {
        private readonly ToyShoppingDbContext _context;

        public CategoryRepository(ToyShoppingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsActive = false;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}