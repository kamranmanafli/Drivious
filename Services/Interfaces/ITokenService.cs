using Drivious.Models;

namespace Drivious.Services.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateAccessToken(AppUser user, IList<string> roles);

        RefreshToken CreateRefreshToken(string userId);
    }
}
