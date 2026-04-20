namespace ToyShoppingWebApp.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer"; // "Customer" or "Admin"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property (one user → many orders)
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}