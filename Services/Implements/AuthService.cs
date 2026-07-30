using Drivious.Constants;
using Drivious.Data;
using Drivious.DTOs.Auth;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            AppDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse> RegisterAsync(RegisterDTO dto)
        {
            var userByUsername = await _userManager.FindByNameAsync(dto.UserName);

            if (userByUsername != null)
            {
                return new ApiResponse(false, "Username already exists.");
            }

            var userByEmail = await _userManager.FindByEmailAsync(dto.Email);

            if (userByEmail != null)
            {
                return new ApiResponse(false, "Email already exists.");
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return new ApiResponse(false, "Passwords do not match.");
            }

            AppUser user = new()
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return new ApiResponse(
                    false,
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            // Self-registration always lands in the least privileged role; an
            // administrator promotes from there.
            await _userManager.AddToRoleAsync(user, AppRoles.Driver);

            return new ApiResponse(true, "User registered successfully.");
        }

        public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                return new ApiResponse<AuthResponseDTO>(
                    false, "Username or password is incorrect.", null);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
            {
                return new ApiResponse<AuthResponseDTO>(
                    false, "Username or password is incorrect.", null);
            }

            var response = await IssueTokensAsync(user);

            return new ApiResponse<AuthResponseDTO>(true, "Login successful.", response);
        }

        public async Task<ApiResponse<AuthResponseDTO>> RefreshAsync(RefreshTokenDTO dto)
        {
            var stored = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (stored == null || !stored.IsActive)
            {
                return new ApiResponse<AuthResponseDTO>(
                    false, "Refresh token is invalid or has expired.", null);
            }

            // Rotate: the presented token is retired as its replacement is issued,
            // so a leaked token cannot be reused.
            stored.RevokedAt = DateTime.UtcNow;

            var response = await IssueTokensAsync(stored.User);

            return new ApiResponse<AuthResponseDTO>(true, "Token refreshed.", response);
        }

        public async Task<ApiResponse> LogoutAsync(RefreshTokenDTO dto)
        {
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (stored == null)
            {
                return new ApiResponse(false, "Refresh token not found.");
            }

            if (stored.RevokedAt == null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return new ApiResponse(true, "Logged out successfully.");
        }

        public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDTO dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return new ApiResponse(false, "New passwords do not match.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ApiResponse(false, "User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                return new ApiResponse(
                    false,
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            // A password change invalidates every outstanding session.
            await RevokeAllTokensAsync(user.Id);

            return new ApiResponse(true, "Password changed successfully.");
        }

        public async Task<ApiResponse<CurrentUserDTO>> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ApiResponse<CurrentUserDTO>(false, "User not found.", null);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new ApiResponse<CurrentUserDTO>(true, "User retrieved successfully.",
                new CurrentUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    DriverId = user.DriverId,
                    Roles = roles.ToList()
                });
        }

        public async Task<ApiResponse> AssignRoleAsync(AssignRoleDTO dto)
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                return new ApiResponse(
                    false,
                    $"Role '{dto.Role}' does not exist. Valid roles: {string.Join(", ", AppRoles.All)}.");
            }

            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                return new ApiResponse(false, "User not found.");
            }

            if (await _userManager.IsInRoleAsync(user, dto.Role))
            {
                return new ApiResponse(false, $"User is already in the {dto.Role} role.");
            }

            // A user holds exactly one role in this application.
            var current = await _userManager.GetRolesAsync(user);

            if (current.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, current);
            }

            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (!result.Succeeded)
            {
                return new ApiResponse(
                    false,
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            // The old access token still carries the previous role until it expires,
            // so force the client to obtain a new one.
            await RevokeAllTokensAsync(user.Id);

            return new ApiResponse(true, $"User assigned to the {dto.Role} role.");
        }

        public async Task<ApiResponse> LinkDriverAsync(LinkDriverDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                return new ApiResponse(false, "User not found.");
            }

            if (dto.DriverId.HasValue)
            {
                var driverExists = await _context.Drivers
                    .AnyAsync(x => x.Id == dto.DriverId.Value && !x.IsDeleted);

                if (!driverExists)
                {
                    return new ApiResponse(false, "Driver not found.");
                }

                var alreadyLinked = await _userManager.Users
                    .AnyAsync(x => x.DriverId == dto.DriverId.Value && x.Id != user.Id);

                if (alreadyLinked)
                {
                    return new ApiResponse(false, "That driver is already linked to another account.");
                }
            }

            user.DriverId = dto.DriverId;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ApiResponse(
                    false,
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            await RevokeAllTokensAsync(user.Id);

            return new ApiResponse(
                true,
                dto.DriverId.HasValue ? "Account linked to driver." : "Account unlinked from driver.");
        }

        private async Task<AuthResponseDTO> IssueTokensAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user, roles);

            var refreshToken = _tokenService.CreateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiresAt = expiresAt,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            };
        }

        private async Task RevokeAllTokensAsync(string userId)
        {
            var active = await _context.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ToListAsync();

            foreach (var token in active)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
