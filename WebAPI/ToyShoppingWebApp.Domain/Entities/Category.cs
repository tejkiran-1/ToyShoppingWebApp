namespace ToyShoppingWebApp.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public ICollection<Toy> Toys { get; set; } = new List<Toy>(); // Navigation property (one category → many toys)
    }
}