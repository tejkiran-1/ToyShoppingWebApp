namespace ToyShoppingWebApp.Application.Services.Interfaces
{
    public interface IUrlShortnerService
    {
        Task<string> ShortUrl(string longUrl);
        Task<string> GetLongUrl(string shortUrl);
    }
}