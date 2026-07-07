using Drivious.Enums;
using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }            // Bildiriş başlığı

        public string Message { get; set; }          // Bildiriş mətni

        public NotificationType Type { get; set; }  // Bildiriş növü

        public bool IsRead { get; set; }             // Oxunub/Oxunmayıb

        public DateTime NotificationDate { get; set; } // Bildiriş tarixi

    }
}
