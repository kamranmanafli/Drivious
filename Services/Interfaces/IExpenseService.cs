using Drivious.DTOs.Expense;

namespace Drivious.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<bool> CreateAsync(ExpenseCreateDTO dto);

        Task<bool> RemoveAsync(Guid id);

        Task<List<ExpenseGetDTO>> GetAllAsync();

        Task<ExpenseGetDTO> GetAsync(Guid id);

        Task<bool> UpdateAsync(Guid id, ExpenseUpdateDTO dto);

        Task<bool> ToggleAsync(Guid id);
    }
}
