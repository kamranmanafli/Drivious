namespace Drivious.Services.Interfaces
{
    /// <summary>
    /// The caller behind the current request. Lets a service narrow a query to the
    /// caller without reaching into HttpContext itself. Identity and roles come from
    /// the access token; the driver link is read from the account, because an
    /// administrator can change it after that token was issued.
    /// </summary>
    public interface ICurrentUser
    {
        string? UserId { get; }

        /// <summary>
        /// The driver record this account is linked to, when there is one. Read from
        /// the account rather than the token, so linking takes effect on the next
        /// request instead of when the caller's current token runs out.
        /// </summary>
        Guid? DriverId { get; }

        /// <summary>
        /// True when the caller is a driver and nothing more. Administrators and
        /// managers see the whole fleet, so they are never restricted by this.
        /// </summary>
        bool IsDriverOnly { get; }
    }
}
