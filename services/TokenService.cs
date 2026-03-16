using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HolyWater.Server.services
{
    public class TokenService:ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) { _config = config; }

        public string CreateAccessToken(UserAccount user, out DateTime expires)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "180");
            expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                // Add other claims/roles as needed
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken CreateRefreshToken(string ipAddress, int userId, DateTime now, int days)
        {
            var rt = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = now.AddDays(days),
                CreatedAt = now,
                CreatedByIp = ipAddress,
                UserId = userId
            };
            return rt;
        }

    }
}
