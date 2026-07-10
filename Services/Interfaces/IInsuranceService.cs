using Drivious.DTOs.Insurance;

namespace Drivious.Services.Interfaces
{
    public interface IInsuranceService
    {
        Task<bool> CreateAsync(InsuranceCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<InsuranceGetDTO>> GetAllAsync();

        Task<InsuranceGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, InsuranceUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
