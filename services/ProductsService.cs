using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;

namespace HolyWater.Server.services
{
    public class ProductsService : IProductsService
    {
        private readonly AppDbContext _context;
        public ProductsService(AppDbContext context)
        {
            _context = context;
        }
        public string SacredText()
        {
            string text = "Immerse yourself in the divine presence with our consecrated Holy Oil and Holy Water, blessed with prayer and reverence. Each bottle is more than a product — it is a sacred tool of spiritual connection, healing, and protection.\r\n\r\nAnointed in faith and prepared with deep spiritual care, our Holy Oil symbolizes God’s healing touch — a reminder that you are covered, guided, and restored by His grace. Use it in times of prayer, over your home, or for moments when you need spiritual strength.\r\n\r\nOur Holy Water, drawn and blessed through scripture and devotion, carries a purity that refreshes the spirit and cleanses the soul. Sprinkle it to protect your space, bless your family, or renew your own spiritual walk.\r\n\r\nThese sacred elements are not just rituals — they are invitations to deeper faith, divine peace, and healing that flows from above.";
            return text;
        }
        public ProductDto[] GetAllProducts()
        {
            var products = _context.Products
       .Select(p => new ProductDto
       {
           Id = p.Id,
           Name = p.Name,
           Price = p.Price,
           Image = p.Image,
           Qty = p.Qty,
           Onsale = p.Onsale,
           Category = p.Category,
           OldPrice = p.OldPrice,
           Description = p.Description,
           Title = p.Title,
           InStock = p.InStock,
           AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.rating) : 0
       })
       .ToArray();

            return products;
        }
       
        public Product? GetProduct(int id)
        {
            return _context.Products.Where(x => x.Id == id).FirstOrDefault();
        }
        public ProductDto[] GetProductsByCategory(string category)
        {
            var products = _context.Products
        .Where(x => x.Category == category)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Image = p.Image,
            Qty = p.Qty,
            Onsale = p.Onsale,
            Category = p.Category,
            OldPrice = p.OldPrice,
            Description = p.Description,
            Title = p.Title,
            InStock = p.InStock,
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.rating) : 0
        })
        .ToArray();

            return products;
        }
    }
    public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; }
    public int Qty { get; set; }
    public bool Onsale { get; set; }
    public string Category { get; set; }
    public decimal? OldPrice { get; set; }
    public string Description { get; set; }
    public string Title { get; set; }
    public bool InStock { get; set; }
    public double AverageRating { get; set; }
}

}
