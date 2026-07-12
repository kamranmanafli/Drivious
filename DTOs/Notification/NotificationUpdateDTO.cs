using Drivious.Enums;

namespace Drivious.DTOs.Notification
{
    public class NotificationUpdateDTO
    {
        public string? Title { get; set; }

        public string? Message { get; set; }

        public NotificationType? Type { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? NotificationDate { get; set; }
    }
}
