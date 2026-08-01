using Drivious.DTOs.Common;
using Drivious.DTOs.Income;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IIncomeService
    {
        Task<ApiResponse> CreateAsync(IncomeCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<IncomeGetDTO>>> GetAllAsync(IncomeQueryParameters parameters);

        Task<ApiResponse<PagedResult<IncomeGetDTO>>> GetDeletedAsync(IncomeQueryParameters parameters);

        Task<ApiResponse<IncomeGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, IncomeUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
