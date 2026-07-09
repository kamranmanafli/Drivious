using Drivious.DTOs.Driver;

namespace Drivious.Services.Interfaces
{
    public interface IDriverService
    {
        Task<bool> CreateAsync(DriverCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<DriverGetDTO>> GetAllAsync();

        Task<DriverGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, DriverUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
