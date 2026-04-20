namespace ToyShoppingWebApp.Domain.Entities
{
    public class Toy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property (many toys → one category)
        public Category Category { get; set; } = null!;
        
        // Navigation property (one toy → many order items)
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}