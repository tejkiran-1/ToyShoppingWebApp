namespace ToyShoppingWebApp.Application.DTOs
{
    public class OrderItemDto
    {
        public int ToyId { get; set; }
        public string ToyName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}