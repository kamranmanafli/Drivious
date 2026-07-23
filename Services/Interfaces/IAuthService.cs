using Drivious.DTOs.Auth;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<object>> RegisterAsync(RegisterDTO dto);

        Task<ApiResponse<string>> LoginAsync(LoginDTO dto);
    }
}
