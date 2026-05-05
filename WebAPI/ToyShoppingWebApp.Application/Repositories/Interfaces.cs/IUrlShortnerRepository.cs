namespace ToyShoppingWebApp.Application.Repositories.Interfaces
{
    public interface IUrlShortnerRepository
    {
        Task<string> ShortUrl(string longUrl, string shortCode);
        Task<string> GetLongUrl(string shortUrl);
    }
}