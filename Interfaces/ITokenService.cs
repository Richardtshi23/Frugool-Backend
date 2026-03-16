using HolyWater.Server.Models;

namespace HolyWater.Server.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(UserAccount user, out DateTime expires);
        RefreshToken CreateRefreshToken(string ipAddress, int userId, DateTime now, int days);
    }
}
