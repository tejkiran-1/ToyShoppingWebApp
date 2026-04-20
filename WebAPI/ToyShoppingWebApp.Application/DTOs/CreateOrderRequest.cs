namespace ToyShoppingWebApp.Application.DTOs
{
    public class CreateOrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public int ToyId { get; set; }
        public int Quantity { get; set; }
    }
}