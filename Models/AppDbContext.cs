using Microsoft.EntityFrameworkCore;

namespace HolyWater.Server.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map Product to existing table
            modelBuilder.Entity<Product>().ToTable("tblProducts");

            // Optional: configure decimal precision for Price fields
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Orders>()
             .HasMany(o => o.Items)
             .WithOne(i => i.Order)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Product>()
            .HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId);

            modelBuilder.Entity<Product>()
                .Property(p => p.OldPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserAccount>()
           .HasOne(u => u.Address)
           .WithOne(a => a.UserAccount)
           .HasForeignKey<Address>(a => a.UserId)
           .IsRequired(false); // <- Make Addre
        }

    }
}
