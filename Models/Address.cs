using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolyWater.Server.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string Line1 { get; set; }
        public string? Line2 { get; set; }
        public string City { get; set; }
        public string ProvinceOrState { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        [ForeignKey("UserId")]
        public UserAccount? UserAccount { get; set; }
        public int? UserId { get; set; }
    }
}
