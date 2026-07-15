using Drivious.DTOs.Maintenance;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IMaintenanceService
    {
        Task<ApiResponse<object>> CreateAsync(MaintenanceCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<MaintenanceGetDTO>>> GetAllAsync();

        Task<ApiResponse<MaintenanceGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, MaintenanceUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
