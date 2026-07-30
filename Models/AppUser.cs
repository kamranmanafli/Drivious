using Microsoft.AspNetCore.Identity;

namespace Drivious.Models
{
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// Set for accounts in the Driver role, linking the login to the driver
        /// record whose assignments and incomes it may read.
        /// </summary>
        public Guid? DriverId { get; set; }

        public Driver? Driver { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
