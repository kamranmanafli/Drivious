using Drivious.DTOs.VehicleAssignment;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleAssignmentService
    {
        Task<ApiResponse> CreateAsync(VehicleAssignmentCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<VehicleAssignmentGetDTO>>> GetAllAsync();

        Task<ApiResponse<List<VehicleAssignmentGetDTO>>> GetDeletedAsync();

        Task<ApiResponse<VehicleAssignmentGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
