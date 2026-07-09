using Drivious.Data;
using Drivious.DTOs.Expense;
using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;

        public ExpenseService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateAsync(ExpenseCreateDTO dto)
        {
            Expense expense = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                Category = dto.Category,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                Description = dto.Description,
            };

            var result = await _context.Expenses.AddAsync(expense);

            if (result.State != EntityState.Added)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<ExpenseGetDTO>> GetAllAsync()
        {
            var expenses = await _context.Expenses.ToListAsync();

            var dtos = expenses.Select(e => new ExpenseGetDTO
            {
                Id = e.Id,

                VehicleId = e.VehicleId,
                Category = e.Category,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Description = e.Description,

                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                DeletedAt = e.DeletedAt,
                IsDeleted = e.IsDeleted

            }).ToList();

            return dtos;
        }

        public async Task<ExpenseGetDTO> GetAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                throw new Exception("Expense not found!");
            }

            var dto = new ExpenseGetDTO()
            {
                Id = expense.Id,

                VehicleId = expense.VehicleId,
                Category = expense.Category,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Description = expense.Description,

                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
                DeletedAt = expense.DeletedAt,
                IsDeleted = expense.IsDeleted
            };

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
                return false;

            var result = _context.Expenses.Remove(expense);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
                return false;

            expense.IsDeleted = !expense.IsDeleted;

            expense.DeletedAt = expense.IsDeleted ? DateTime.Now : null;

            var result = _context.Expenses.Update(expense);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
        public async Task<bool> UpdateAsync(Guid id, ExpenseUpdateDTO dto)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
                return false;

            expense.VehicleId = dto.VehicleId ?? expense.VehicleId;

            expense.Category = dto.Category ?? expense.Category;

            expense.Amount = dto.Amount ?? expense.Amount;

            expense.ExpenseDate = dto.ExpenseDate ?? expense.ExpenseDate;

            expense.Description = dto.Description ?? expense.Description;

            expense.UpdatedAt = DateTime.Now;

            var result = _context.Expenses.Update(expense);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
