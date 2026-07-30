namespace Drivious.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;

        /// <summary>Random opaque value handed to the client; not a JWT.</summary>
        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public AppUser User { get; set; } = null!;

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
