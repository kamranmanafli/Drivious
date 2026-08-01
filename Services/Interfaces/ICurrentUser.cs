namespace Drivious.Services.Interfaces
{
    /// <summary>
    /// The caller behind the current request, read from the access token. Lets a
    /// service narrow a query to the caller without reaching into HttpContext itself.
    /// </summary>
    public interface ICurrentUser
    {
        string? UserId { get; }

        /// <summary>The driver record this account is linked to, when there is one.</summary>
        Guid? DriverId { get; }

        /// <summary>
        /// True when the caller is a driver and nothing more. Administrators and
        /// managers see the whole fleet, so they are never restricted by this.
        /// </summary>
        bool IsDriverOnly { get; }
    }
}
