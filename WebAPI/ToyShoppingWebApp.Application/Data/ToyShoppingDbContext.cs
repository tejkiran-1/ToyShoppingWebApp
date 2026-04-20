using Microsoft.EntityFrameworkCore;
using ToyShoppingWebApp.Domain.Entities;

namespace ToyShoppingWebApp.Application.Data
{
    /// <summary>
    /// DbContext for Toy Shopping Web Application
    /// Represents the database and configures entity relationships
    /// </summary>
    public class ToyShoppingDbContext : DbContext
    {
        public ToyShoppingDbContext(DbContextOptions<ToyShoppingDbContext> options)
            : base(options)
        {
        }

        // DbSets represent tables in the database
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Toy> Toys { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        /// <summary>
        /// Configure entity mappings, relationships, and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== USER CONFIGURATION ==========
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                // Email must be unique (no two users with same email)
                entity.HasIndex(u => u.Email).IsUnique();

                // Email is required
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                // Password hash is required
                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(512);

                // Role is required
                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                // One User → Many Orders
                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.User) // WithOne means Order has a reference to User
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete user → delete their orders
            });

            // ========== CATEGORY CONFIGURATION ==========
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Description)
                    .HasMaxLength(500);

                // One Category → Many Toys
                entity.HasMany(c => c.Toys)
                    .WithOne(t => t.Category)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete category if toys exist
            });

            // ========== TOY CONFIGURATION ==========
            modelBuilder.Entity<Toy>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .HasMaxLength(1000);

                // Price must have precision (2 decimal places for currency)
                entity.Property(t => t.Price)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                // Stock cannot be negative
                entity.Property(t => t.Stock)
                    .IsRequired();

                // One Toy → Many OrderItems
                entity.HasMany(t => t.OrderItems)
                    .WithOne(oi => oi.Toy)
                    .HasForeignKey(oi => oi.ToyId)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete toy if order items exist
            });

            // ========== ORDER CONFIGURATION ==========
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.TotalPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(o => o.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                // One Order → Many OrderItems
                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete order → delete order items
            });

            // ========== ORDER ITEM CONFIGURATION ==========
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.Quantity)
                    .IsRequired();

                entity.Property(oi => oi.UnitPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                // Composite index for efficient queries
                entity.HasIndex(oi => new { oi.OrderId, oi.ToyId });
            });
        }
    }
}
