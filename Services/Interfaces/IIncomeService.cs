using Drivious.DTOs.Income;

namespace Drivious.Services.Interfaces
{
    public interface IIncomeService
    {
        Task<bool> CreateAsync(IncomeCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<IncomeGetDTO>> GetAllAsync();

        Task<IncomeGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, IncomeUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
