using Drivious.Constants;
using Drivious.DTOs.Auth;
using Drivious.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drivious.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthsController(IAuthService authService)
        {
            _authService = authService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var result = await _authService.LoginAsync(dto);

            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenDTO dto)
        {
            var result = await _authService.RefreshAsync(dto);

            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenDTO dto)
        {
            var result = await _authService.LogoutAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _authService.GetCurrentUserAsync(UserId);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            var result = await _authService.ChangePasswordAsync(UserId, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lists the accounts an administrator manages. Without it there is no way to
        /// discover which users exist - assign-role and link-driver both take a user
        /// name the caller has to already know.
        /// </summary>
        [Authorize(Roles = AppRoles.Admin)]
        [HttpGet("users")]
        public async Task<IActionResult> Users([FromQuery] UserQueryParameters parameters)
        {
            var result = await _authService.GetUsersAsync(parameters);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(AssignRoleDTO dto)
        {
            var result = await _authService.AssignRoleAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost("link-driver")]
        public async Task<IActionResult> LinkDriver(LinkDriverDTO dto)
        {
            var result = await _authService.LinkDriverAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
