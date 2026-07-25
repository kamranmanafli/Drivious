using Drivious.DTOs.Notification;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse> CreateAsync(NotificationCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<NotificationGetDTO>>> GetAllAsync();

        Task<ApiResponse<NotificationGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, NotificationUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
