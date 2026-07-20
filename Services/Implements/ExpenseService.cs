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

        public async Task<ApiResponse<object>> CreateAsync(ExpenseCreateDTO dto)
        {
            Expense expense = _mapper.Map<Expense>(dto);

            expense.CreatedAt = DateTime.Now;

            var result = await _context.Expenses.AddAsync(expense);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Expense created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<ExpenseGetDTO>>> GetAllAsync()
        {
            var expenses = await _context.Expenses.ToListAsync();

            var dtos = _mapper.Map<List<ExpenseGetDTO>>(expenses);

            return new ApiResponse<List<ExpenseGetDTO>>(
                true,
                "Expenses retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<ExpenseGetDTO>> GetAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

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

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense not found.",
                    null
                );
            }

            var result = _context.Expenses.Remove(expense);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Expense deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense not found.",
                    null
                );
            }

            expense.IsDeleted = !expense.IsDeleted;
            expense.DeletedAt = expense.IsDeleted ? DateTime.Now : null;

            var result = _context.Expenses.Update(expense);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Expense status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, ExpenseUpdateDTO dto)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense not found.",
                    null
                );
            }

            _mapper.Map(dto, expense);

            expense.UpdatedAt = DateTime.Now;

            var result = _context.Expenses.Update(expense);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Expense could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Expense updated successfully.",
                null
            );
        }
    }
}
