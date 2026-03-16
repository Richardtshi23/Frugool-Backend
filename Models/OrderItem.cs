using Org.BouncyCastle.Asn1.X509;

namespace HolyWater.Server.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; 
        public string? ProductImage { get; set; }  
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public Orders Order { get; set; }
    }
}
