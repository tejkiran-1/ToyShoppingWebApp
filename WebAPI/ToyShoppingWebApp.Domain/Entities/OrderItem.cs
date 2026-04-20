namespace ToyShoppingWebApp.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ToyId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        
        // Navigation properties
        public Order Order { get; set; } = null!;
        public Toy Toy { get; set; } = null!;
    }
}