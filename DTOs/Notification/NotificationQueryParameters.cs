using Drivious.DTOs.Common;
using Drivious.Enums;

namespace Drivious.DTOs.Notification
{
    public class NotificationQueryParameters : QueryParameters
    {
        /// <summary>Matched against the title and the message body.</summary>
        public NotificationType? Type { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}
