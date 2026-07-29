using Drivious.DTOs.Income;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IIncomeService
    {
        Task<ApiResponse> CreateAsync(IncomeCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<IncomeGetDTO>>> GetAllAsync();

        Task<ApiResponse<List<IncomeGetDTO>>> GetDeletedAsync();

        Task<ApiResponse<IncomeGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, IncomeUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
