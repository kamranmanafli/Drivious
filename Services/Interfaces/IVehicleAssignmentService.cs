using Drivious.DTOs.Common;
using Drivious.DTOs.VehicleAssignment;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleAssignmentService
    {
        Task<ApiResponse> CreateAsync(VehicleAssignmentCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<VehicleAssignmentGetDTO>>> GetAllAsync(
            VehicleAssignmentQueryParameters parameters);

        Task<ApiResponse<PagedResult<VehicleAssignmentGetDTO>>> GetDeletedAsync(
            VehicleAssignmentQueryParameters parameters);

        Task<ApiResponse<VehicleAssignmentGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);

        Task<ApiResponse> ReturnAsync(Guid id, DateTime? returnedDate);
    }
}
