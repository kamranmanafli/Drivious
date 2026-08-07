namespace Drivious.DTOs.Auth
{
    public class RegisterDTO
    {
        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;

        /// <summary>
        /// Which role to open the account in. Empty means Driver — the only role
        /// anyone may take without proving anything.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Required when <see cref="Role"/> asks for more than Driver. Checked
        /// against Registration:InviteCode, which is set per environment.
        /// </summary>
        public string? InviteCode { get; set; }
    }
}
