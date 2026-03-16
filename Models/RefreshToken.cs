namespace HolyWater.Server.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; }
        public int UserId { get; set; }
        public UserAccount? User { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = "";
    }
}
