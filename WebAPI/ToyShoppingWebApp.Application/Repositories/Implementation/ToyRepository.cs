namespace ToyShoppingWebApp.Application.Repositories.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using ToyShoppingWebApp.Application.Data;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class ToyRepository : IToyRepository
    {
        private readonly ToyShoppingDbContext _context;

        public ToyRepository(ToyShoppingDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Toy>, int)> GetToysByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var query = _context.Toys
                .AsNoTracking()
                .Where(t => t.CategoryId == categoryId && t.IsActive);

            int totalCount = await query.CountAsync();

            var toys = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (toys, totalCount);
        }

        public async Task<List<Toy>> GetAllToysAsync()
        {
            return await _context.Toys
                .AsNoTracking()
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        public async Task<Toy?> GetToyByIdAsync(int id)
        {
            return await _context.Toys
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Toy> CreateToyAsync(Toy toy)
        {
            _context.Toys.Add(toy);
            await _context.SaveChangesAsync();
            return toy;
        }

        public async Task<Toy> UpdateToyAsync(Toy toy)
        {
            _context.Toys.Update(toy);
            await _context.SaveChangesAsync();
            return toy;
        }

        public async Task DeleteToyAsync(int id)
        {
            var toy = await _context.Toys.FindAsync(id);
            if (toy != null)
            {
                toy.IsActive = false;
                _context.Toys.Update(toy);
                await _context.SaveChangesAsync();
            }
        }
    }
}