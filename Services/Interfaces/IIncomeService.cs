using Drivious.DTOs.Income;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IIncomeService
    {
        Task<ApiResponse<object>> CreateAsync(IncomeCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<IncomeGetDTO>>> GetAllAsync();

        Task<ApiResponse<IncomeGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, IncomeUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
