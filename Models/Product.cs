using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolyWater.Server.Models
{
    [Table("tblProducts")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public string Image { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool InStock { get; set; }
        public bool Onsale { get; set; }
        public int Qty { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
