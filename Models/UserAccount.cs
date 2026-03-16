using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolyWater.Server.Models
{
    [Table("tblUserAccount")]
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string DateOfBirth { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; } = new();
        public int ContactNumber { get; set; }
        public string Gender { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
        public string? RoleName { get; set; }
        public Address? Address { get; set; }
        public List<Orders>? Orders { get; set; } = new();
        public List<Review> reviews { get; set; } = new();
    }
}
