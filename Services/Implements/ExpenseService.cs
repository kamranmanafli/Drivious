using AutoMapper;
using Drivious.Data;
using Drivious.DTOs.Expense;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ExpenseService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse> CreateAsync(ExpenseCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            Expense expense = _mapper.Map<Expense>(dto);

            expense.CreatedAt = DateTime.UtcNow;

            var result = await _context.Expenses.AddAsync(expense);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Expense could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Expense could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Expense created successfully."
            );
        }

        public async Task<ApiResponse<List<ExpenseGetDTO>>> GetAllAsync()
        {
            var expenses = await _context.Expenses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<ExpenseGetDTO>>(expenses);

            return new ApiResponse<List<ExpenseGetDTO>>(
                true,
                "Expenses retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<ExpenseGetDTO>>> GetDeletedAsync()
        {
            var expenses = await _context.Expenses
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<ExpenseGetDTO>>(expenses);

            return new ApiResponse<List<ExpenseGetDTO>>(
                true,
                "Deleted expenses retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<ExpenseGetDTO>> GetAsync(Guid id)
        {
            var expense = await _context.Expenses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (expense == null)
            {
                return new ApiResponse<ExpenseGetDTO>(
                    false,
                    "Expense not found.",
                    null
                );
            }

            var dto = _mapper.Map<ExpenseGetDTO>(expense);

            return new ApiResponse<ExpenseGetDTO>(
                true,
                "Expense retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return new ApiResponse(
                    false,
                    "Expense not found."
                );
            }

            var result = _context.Expenses.Remove(expense);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Expense could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Expense could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Expense deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return new ApiResponse(
                    false,
                    "Expense not found."
                );
            }

            expense.IsDeleted = !expense.IsDeleted;
            expense.DeletedAt = expense.IsDeleted ? DateTime.UtcNow : null;

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a vehicle restore resurrect this row.
            expense.DeletedByCascade = false;

            var result = _context.Expenses.Update(expense);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Expense status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Expense status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Expense status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, ExpenseUpdateDTO dto)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return new ApiResponse(
                    false,
                    "Expense not found."
                );
            }

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            _mapper.Map(dto, expense);

            expense.UpdatedAt = DateTime.UtcNow;

            var result = _context.Expenses.Update(expense);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Expense could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Expense could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Expense updated successfully."
            );
        }
    }
}
