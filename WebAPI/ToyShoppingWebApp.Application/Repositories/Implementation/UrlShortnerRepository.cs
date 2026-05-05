using ToyShoppingWebApp.Application.Data;
using ToyShoppingWebApp.Application.Repositories.Interfaces;
using ToyShoppingWebApp.Domain.Entities;

namespace ToyShoppingWebApp.Application.Repositories.Implementations
{
    public class UrlShortnerRepository : IUrlShortnerRepository
    {
        private readonly ToyShoppingDbContext _context;

        public UrlShortnerRepository(ToyShoppingDbContext context)
        {
            _context = context;
        }

        public Task<string> GetLongUrl(string shortUrl)
        {
            var urlMapping = _context.UrlMappings.FirstOrDefault(m => m.ShortCode == shortUrl);
            if (urlMapping != null)
            {
                urlMapping.LastAccessedDate = DateTime.UtcNow;
                _context.SaveChanges();
            }
            return Task.FromResult(urlMapping?.LongUrl);
        }

        public Task<string> ShortUrl(string longUrl, string shortCode)
        {
            if (_context.UrlMappings.Any(m => m.ShortCode == shortCode))
            {
                return Task.FromResult<string>(shortCode); // Return existing short code if it already exists
            }

            var urlMapping = new UrlMapping
            {
                LongUrl = longUrl,
                ShortCode = shortCode,
                CreatedDate = DateTime.UtcNow,
                LastAccessedDate = DateTime.UtcNow,
                AccessCount = 0
            };

            _context.UrlMappings.Add(urlMapping);
            _context.SaveChanges();

            return Task.FromResult(urlMapping.ShortCode);
        }
    }
}