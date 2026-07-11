using Drivious.DTOs.VehicleAssignment;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleAssignmentService
    {
        Task<bool> CreateAsync(VehicleAssignmentCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<VehicleAssignmentGetDTO>> GetAllAsync();

        Task<VehicleAssignmentGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
