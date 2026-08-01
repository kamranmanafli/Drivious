using Drivious.DTOs.Common;
using Drivious.DTOs.VehicleDocument;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleDocumentService
    {
        Task<ApiResponse> CreateAsync(VehicleDocumentCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<VehicleDocumentGetDTO>>> GetAllAsync(
            VehicleDocumentQueryParameters parameters);

        Task<ApiResponse<PagedResult<VehicleDocumentGetDTO>>> GetDeletedAsync(
            VehicleDocumentQueryParameters parameters);

        Task<ApiResponse<VehicleDocumentGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, VehicleDocumentUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
