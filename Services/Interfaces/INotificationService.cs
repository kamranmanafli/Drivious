using Drivious.DTOs.Notification;

namespace Drivious.Services.Interfaces
{
    public interface INotificationService
    {
        Task<bool> CreateAsync(NotificationCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<NotificationGetDTO>> GetAllAsync();

        Task<NotificationGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, NotificationUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
