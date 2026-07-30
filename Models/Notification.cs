using Drivious.Enums;
using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = null!;            // Bildiriş başlığı

        public string Message { get; set; } = null!;          // Bildiriş mətni

        public NotificationType Type { get; set; }  // Bildiriş növü

        public bool IsRead { get; set; }             // Oxunub/Oxunmayıb

        public DateTime NotificationDate { get; set; } // Bildiriş tarixi

        /// <summary>
        /// Stable key for notifications raised by the background generator, so the
        /// same warning is not re-created on every run. Null for manual entries.
        /// Shape: "reason:entityId:relevantDate", e.g. "insurance-expiring:{id}:20260815".
        /// </summary>
        public string? ReferenceKey { get; set; }
    }
}
