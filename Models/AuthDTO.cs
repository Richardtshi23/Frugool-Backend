namespace HolyWater.Server.Models
{
    public class AuthDTO
    {
        public record RegisterDto(string email, string password, string username ,int contactNumber, string gender );
        public record LoginDto(string Email, string Password);
        public record AuthResponseDto(string AccessToken, DateTime ExpiresAt, string Email);
    }
}
