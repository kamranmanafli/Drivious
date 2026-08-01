using Drivious.DTOs.Common;
using Drivious.DTOs.Insurance;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IInsuranceService
    {
        Task<ApiResponse> CreateAsync(InsuranceCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<InsuranceGetDTO>>> GetAllAsync(InsuranceQueryParameters parameters);

        Task<ApiResponse<PagedResult<InsuranceGetDTO>>> GetDeletedAsync(InsuranceQueryParameters parameters);

        Task<ApiResponse<InsuranceGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, InsuranceUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
