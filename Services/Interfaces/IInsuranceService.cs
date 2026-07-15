using Drivious.DTOs.Insurance;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IInsuranceService
    {
        Task<ApiResponse<object>> CreateAsync(InsuranceCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<InsuranceGetDTO>>> GetAllAsync();

        Task<ApiResponse<InsuranceGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, InsuranceUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
