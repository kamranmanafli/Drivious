using Drivious.DTOs.FuelLog;

namespace Drivious.Services.Interfaces
{
    public interface IFuelLogService
    {
        Task<bool> CreateAsync(FuelLogCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<FuelLogGetDTO>> GetAllAsync();

        Task<FuelLogGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, FuelLogUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
