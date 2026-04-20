namespace ToyShoppingWebApp.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "Completed", "Cancelled"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property (many orders → one user)
        public User User { get; set; } = null!;
        
        // Navigation property (one order → many order items)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}