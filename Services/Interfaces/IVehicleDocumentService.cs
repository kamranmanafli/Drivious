using Drivious.DTOs.VehicleDocumnet;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleDocumentService
    {
        Task<bool> CreateAsync(VehicleDocumentCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<VehicleDocumentGetDTO>> GetAllAsync();

        Task<VehicleDocumentGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, VehicleDocumentUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
