namespace Drivious.Services.Interfaces
{
    /// <summary>
    /// Turns dates that are about to pass - insurance, driving licences, scheduled
    /// service and document expiry - into notifications.
    /// </summary>
    public interface INotificationGenerator
    {
        /// <summary>
        /// Scans the fleet and inserts the notifications that do not exist yet.
        /// Returns how many were created, so a caller can report the result.
        /// </summary>
        Task<int> GenerateAsync(CancellationToken cancellationToken = default);
    }
}
