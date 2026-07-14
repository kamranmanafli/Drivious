using Drivious.DTOs.Expense;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<ApiResponse<object>> CreateAsync(ExpenseCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<ExpenseGetDTO>>> GetAllAsync();

        Task<ApiResponse<ExpenseGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, ExpenseUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
