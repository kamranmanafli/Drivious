using Drivious.DTOs.Vehicle;

namespace Drivious.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<bool> CreateAsync(VehicleCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<VehicleGetDTO>> GetAllAsync();

        Task<VehicleGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, VehicleUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
