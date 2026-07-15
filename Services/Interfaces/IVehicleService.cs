using Drivious.DTOs.Vehicle;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<ApiResponse<object>> CreateAsync(VehicleCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<VehicleGetDTO>>> GetAllAsync();

        Task<ApiResponse<VehicleGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, VehicleUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
