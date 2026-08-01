using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.Expense;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<Expense, object?>>> Sortable =
            new Dictionary<string, Expression<Func<Expense, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["amount"] = x => x.Amount,
                ["category"] = x => x.Category,
                ["expenseDate"] = x => x.ExpenseDate,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<Expense> BuildQuery(ExpenseQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.Expenses
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.Category.HasValue, x => x.Category == parameters.Category!.Value)
                .WhereIf(parameters.From.HasValue, x => x.ExpenseDate >= parameters.From!.Value)
                .WhereIf(parameters.To.HasValue, x => x.ExpenseDate <= parameters.To!.Value)
                .WhereIf(parameters.MinAmount.HasValue, x => x.Amount >= parameters.MinAmount!.Value)
                .WhereIf(parameters.MaxAmount.HasValue, x => x.Amount <= parameters.MaxAmount!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => x.Description.Contains(search!)
                      || x.Vehicle.PlateNumber.Contains(search!))
                .ApplySort(parameters, Sortable, "expenseDate");
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

        public async Task<ApiResponse<PagedResult<ExpenseGetDTO>>> GetAllAsync(ExpenseQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<Expense, ExpenseGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<ExpenseGetDTO>>(
                true,
                "Expenses retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<ExpenseGetDTO>>> GetDeletedAsync(ExpenseQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<Expense, ExpenseGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<ExpenseGetDTO>>(
                true,
                "Deleted expenses retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<ExpenseGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.Expenses
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<ExpenseGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<ExpenseGetDTO>(
                    false,
                    "Expense not found.",
                    null
                );
            }

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
