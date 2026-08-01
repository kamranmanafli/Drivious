using Drivious.DTOs.Common;
using Drivious.DTOs.Expense;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<ApiResponse> CreateAsync(ExpenseCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<ExpenseGetDTO>>> GetAllAsync(ExpenseQueryParameters parameters);

        Task<ApiResponse<PagedResult<ExpenseGetDTO>>> GetDeletedAsync(ExpenseQueryParameters parameters);

        Task<ApiResponse<ExpenseGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, ExpenseUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
