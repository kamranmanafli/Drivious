using Drivious.DTOs.Notification;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<object>> CreateAsync(NotificationCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<NotificationGetDTO>>> GetAllAsync();

        Task<ApiResponse<NotificationGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, NotificationUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
