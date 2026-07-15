using Drivious.DTOs.VehicleAssignment;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleAssignmentService
    {
        Task<ApiResponse<object>> CreateAsync(VehicleAssignmentCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<VehicleAssignmentGetDTO>>> GetAllAsync();

        Task<ApiResponse<VehicleAssignmentGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
