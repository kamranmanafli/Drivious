using Drivious.DTOs.Auth;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Drivious.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse> RegisterAsync(RegisterDTO dto)
        {
            var userByUsername = await _userManager.FindByNameAsync(dto.UserName);

            if (userByUsername != null)
            {
                return new ApiResponse(
                    false,
                    "Username already exists."
                );
            }

            var userByEmail = await _userManager.FindByEmailAsync(dto.Email);

            if (userByEmail != null)
            {
                return new ApiResponse(
                    false,
                    "Email already exists."
                );
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return new ApiResponse(
                    false,
                    "Passwords do not match."
                );
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
                    string.Join(", ", result.Errors.Select(x => x.Description))
                );
            }

            return new ApiResponse(
                true,
                "User registered successfully."
            );
        }

        public async Task<ApiResponse<string>> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                return new ApiResponse<string>(
                    false,
                    "Username or password is incorrect.",
                    null
                );
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
            {
                return new ApiResponse<string>(
                    false,
                    "Username or password is incorrect.",
                    null
                );
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _tokenService.CreateToken(user, roles);

            return new ApiResponse<string>(
                true,
                "Login successful.",
                token
            );
        }
    }
}
