using Drivious.DTOs.Vehicle;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<ApiResponse> CreateAsync(VehicleCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<VehicleGetDTO>>> GetAllAsync();

        Task<ApiResponse<List<VehicleGetDTO>>> GetDeletedAsync();

        Task<ApiResponse<VehicleGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, VehicleUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
