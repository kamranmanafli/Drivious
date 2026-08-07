using Drivious.DTOs.Auth;
using Drivious.DTOs.Common;
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

        /// <summary>
        /// The accounts an administrator manages, with their roles and driver link.
        /// </summary>
        Task<ApiResponse<PagedResult<UserGetDTO>>> GetUsersAsync(UserQueryParameters parameters);

        Task<ApiResponse> AssignRoleAsync(AssignRoleDTO dto);

        Task<ApiResponse> LinkDriverAsync(LinkDriverDTO dto);
    }
}
