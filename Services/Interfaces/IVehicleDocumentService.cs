using Drivious.DTOs.VehicleDocumnet;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleDocumentService
    {
        Task<ApiResponse> CreateAsync(VehicleDocumentCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetAllAsync();

        Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetDeletedAsync();

        Task<ApiResponse<VehicleDocumentGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, VehicleDocumentUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
