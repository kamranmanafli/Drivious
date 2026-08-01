using Drivious.DTOs.Common;
using Drivious.DTOs.Vehicle;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<ApiResponse> CreateAsync(VehicleCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<VehicleGetDTO>>> GetAllAsync(VehicleQueryParameters parameters);

        Task<ApiResponse<PagedResult<VehicleGetDTO>>> GetDeletedAsync(VehicleQueryParameters parameters);

        Task<ApiResponse<VehicleGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, VehicleUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
