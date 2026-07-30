namespace Drivious.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime AccessTokenExpiresAt { get; set; }

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public List<string> Roles { get; set; } = new();
    }
}
