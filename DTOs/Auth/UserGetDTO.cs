namespace Drivious.DTOs.Auth
{
    /// <summary>
    /// One account as the administration screen lists it. The password hash and
    /// the security stamp are deliberately absent - nothing here is a secret.
    /// </summary>
    public class UserGetDTO
    {
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        /// <summary>Set when the account is tied to a driver record.</summary>
        public Guid? DriverId { get; set; }

        /// <summary>Resolved from the linked driver, so the list reads without a second call.</summary>
        public string? DriverFullName { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
