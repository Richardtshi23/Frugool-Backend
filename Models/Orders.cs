using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolyWater.Server.Models
{
    [Table("tblOrders")]
    public class Orders
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string Reference { get; set; }   
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public bool isFulfilled { get; set; }

    }
}
