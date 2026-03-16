namespace HolyWater.Server.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }  // Foreign key to Product
        public Product Product { get; set; }  // Navigation property
        public string Name { get; set; }  // Optional: user name
        public string Comment { get; set; }
        public int rating { get; set; }  // 1-5
        public int UserId { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
