using ToyShoppingWebApp.Application.Repositories.Interfaces;
using ToyShoppingWebApp.Application.Services.Interfaces;

namespace ToyShoppingWebApp.Application.Services.Implementations
{
    public class UrlShortnerService : IUrlShortnerService
    {
        IUrlShortnerRepository urlShortnerRepository;

        public UrlShortnerService(IUrlShortnerRepository repository)
        {
            urlShortnerRepository = repository;
        }

        public Task<string> ShortUrl(string longUrl)
        {
            var shortCode = GenerateShortCode();
            return urlShortnerRepository.ShortUrl(longUrl, shortCode);
        }

        public Task<string> GetLongUrl(string shortUrl)
        {
            return urlShortnerRepository.GetLongUrl(shortUrl);
        }



        private static string GenerateShortCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890";
            var random = new Random();
            var shortCode = new char[6];

            for (int i = 0; i < shortCode.Length; i++)
            {
                shortCode[i] = chars[random.Next(chars.Length)];
            }
            return new string(shortCode);
        }

        
    }
}