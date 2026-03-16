namespace HolyWater.Server.Models
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool InStock { get; set; }
        public bool OnSale { get; set; }
        public int Qty { get; set; }
        public IFormFile Image { get; set; }
    }
}
