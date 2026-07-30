namespace Drivious.Models
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;

        /// <summary>
        /// Lifetime of the access token. Kept short because it cannot be revoked;
        /// clients renew it with a refresh token.
        /// </summary>
        public int ExpireDays { get; set; } = 1;

        public int RefreshTokenExpireDays { get; set; } = 30;
    }
}
