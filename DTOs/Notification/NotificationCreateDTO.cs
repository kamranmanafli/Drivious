using Drivious.Enums;

namespace Drivious.DTOs.Notification
{
    public class NotificationCreateDTO
    {
        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime NotificationDate { get; set; }
    }
}
