using Drivious.DTOs.Maintenance;

namespace Drivious.Services.Interfaces
{
    public interface IMaintenanceService
    {
        Task<bool> CreateAsync(MaintenanceCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<MaintenanceGetDTO>> GetAllAsync();

        Task<MaintenanceGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, MaintenanceUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
