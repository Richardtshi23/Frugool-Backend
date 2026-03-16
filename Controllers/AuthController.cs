using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;
using HolyWater.Server.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static HolyWater.Server.Models.AuthDTO;

namespace HolyWater.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokens;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, ITokenService tokens, IConfiguration config)
        {
            _db = db;
            _tokens = tokens;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(userDTO dto)
        {
            var exists = await _db.UserAccounts.AnyAsync(u => u.Email == dto.email);
            if (exists) return BadRequest("Email already registered.");

            var user = new UserAccount
            {
                Email = dto.email,
                Username = dto.username,
                ContactNumber = dto.contactNumber,
                Gender = dto.gender,
                DateOfBirth = dto.dateOfBirth,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
                Address = new Address
                {
                    Line1 = dto.address.line1,
                    Line2 = dto.address.line2,
                    City = dto.address.city,
                    ProvinceOrState = dto.address.province,
                    PostalCode = dto.address.postalCode,
                    Country = dto.address.country
                }
                };
            _db.UserAccounts.Add(user);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.UserAccounts.Include(u => u.RefreshTokens).SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var accessToken = _tokens.CreateAccessToken(user, out var accessExpiresAt);
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "14");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var refreshToken = _tokens.CreateRefreshToken(ip, user.Id, DateTime.UtcNow, refreshDays);
            user.RefreshTokens?.Add(refreshToken);
            await _db.SaveChangesAsync();

            // Set HttpOnly cookie for refresh token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = refreshToken.ExpiresAt,
                Path = "/"
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            return Ok(new AuthResponseDto(accessToken, accessExpiresAt, user.Email));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var token)) return Unauthorized();

            var stored = await _db.RefreshTokens.Include(rt => rt.User).SingleOrDefaultAsync(rt => rt.Token == token);
            if (stored == null || stored.ExpiresAt <= DateTime.UtcNow || stored.Revoked) return Unauthorized();

            // create new access token
            var user = stored.User!;
            var accessToken = _tokens.CreateAccessToken(user, out var accessExpiresAt);

            // Optionally rotate refresh tokens: revoke old and create new
            stored.Revoked = true;
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "14");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var newRt = _tokens.CreateRefreshToken(ip, user.Id, DateTime.UtcNow, refreshDays);
            user.RefreshTokens?.Add(newRt);
            await _db.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRt.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newRt.ExpiresAt,
                Path = "/api/auth"
            });

            return Ok(new AuthResponseDto(accessToken, accessExpiresAt, user.Email));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var token)) { /* just clear cookie */ }

            if (token != null)
            {
                var stored = await _db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token);
                if (stored != null)
                {
                    stored.Revoked = true;
                    await _db.SaveChangesAsync();
                }
            }

            // Clear cookie
            var cookieOptions = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddDays(-1), Path = "/api/auth" };
            Response.Cookies.Append("refreshToken", "", cookieOptions);
            return Ok();
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");
            var user = await _db.UserAccounts
       .Where(x => x.Id == userId)
       .Select(x => new {
           id = x.Id,
           email = x.Email,
           fullName = x.Username,
           phone = x.ContactNumber,
           address = x.Address.Line1 + x.Address.Line2,
           city = x.Address.City,
           province = x.Address.ProvinceOrState,
           postalCode = x.Address.PostalCode
       })
       .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
