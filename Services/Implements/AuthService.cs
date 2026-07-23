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

        public async Task<ApiResponse<object>> RegisterAsync(RegisterDTO dto)
        {
            var userByUsername = await _userManager.FindByNameAsync(dto.UserName);

            if (userByUsername != null)
            {
                return new ApiResponse<object>(
                    false,
                    "Username already exists.",
                    null
                );
            }

            var userByEmail = await _userManager.FindByEmailAsync(dto.Email);

            if (userByEmail != null)
            {
                return new ApiResponse<object>(
                    false,
                    "Email already exists.",
                    null
                );
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return new ApiResponse<object>(
                    false,
                    "Passwords do not match.",
                    null
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
                return new ApiResponse<object>(
                    false,
                    string.Join(", ", result.Errors.Select(x => x.Description)),
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "User registered successfully.",
                null
            );
        }

        public Task<ApiResponse<string>> LoginAsync(LoginDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}
