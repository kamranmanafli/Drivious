using Drivious.DTOs.Auth;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(RegisterDTO dto);

        Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto);

        Task<ApiResponse<AuthResponseDTO>> RefreshAsync(RefreshTokenDTO dto);

        Task<ApiResponse> LogoutAsync(RefreshTokenDTO dto);

        Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDTO dto);

        Task<ApiResponse<CurrentUserDTO>> GetCurrentUserAsync(string userId);

        Task<ApiResponse> AssignRoleAsync(AssignRoleDTO dto);

        Task<ApiResponse> LinkDriverAsync(LinkDriverDTO dto);
    }
}
