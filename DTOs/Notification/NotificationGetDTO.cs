using Drivious.Enums;

namespace Drivious.DTOs.Notification
{
    public class NotificationGetDTO
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime NotificationDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
