using Drivious.DTOs.VehicleDocumnet;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleDocumentService
    {
        Task<ApiResponse<object>> CreateAsync(VehicleDocumentCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetAllAsync();

        Task<ApiResponse<VehicleDocumentGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, VehicleDocumentUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
