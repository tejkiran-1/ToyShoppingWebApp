namespace ToyShoppingWebApp.Domain.Entities
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string ShortCode { get; set; } = null!;
        public string LongUrl { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedDate { get; set; } = DateTime.UtcNow;
        public int AccessCount { get; set; } = 0;
    }
}

