using Drivious.DTOs.Maintenance;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IMaintenanceService
    {
        Task<ApiResponse> CreateAsync(MaintenanceCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<MaintenanceGetDTO>>> GetAllAsync();

        Task<ApiResponse<List<MaintenanceGetDTO>>> GetDeletedAsync();

        Task<ApiResponse<MaintenanceGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, MaintenanceUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
