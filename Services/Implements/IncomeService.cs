using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.Income;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Drivious.Services.Implements
{
    public class IncomeService : IIncomeService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public IncomeService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<Income, object?>>> Sortable =
            new Dictionary<string, Expression<Func<Income, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["amount"] = x => x.Amount,
                ["incomeDate"] = x => x.IncomeDate,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["driver"] = x => x.Driver.FirstName,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<Income> BuildQuery(IncomeQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.Incomes
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.DriverId.HasValue, x => x.DriverId == parameters.DriverId!.Value)
                .WhereIf(parameters.From.HasValue, x => x.IncomeDate >= parameters.From!.Value)
                .WhereIf(parameters.To.HasValue, x => x.IncomeDate <= parameters.To!.Value)
                .WhereIf(parameters.MinAmount.HasValue, x => x.Amount >= parameters.MinAmount!.Value)
                .WhereIf(parameters.MaxAmount.HasValue, x => x.Amount <= parameters.MaxAmount!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => (x.Description != null && x.Description.Contains(search!))
                      || x.Vehicle.PlateNumber.Contains(search!))
                .ApplySort(parameters, Sortable, "incomeDate");
        }

        public async Task<ApiResponse> CreateAsync(IncomeCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId, dto.DriverId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            Income income = _mapper.Map<Income>(dto);

            income.CreatedAt = DateTime.UtcNow;

            var result = await _context.Incomes.AddAsync(income);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Income could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Income could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Income created successfully."
            );
        }

        public async Task<ApiResponse<PagedResult<IncomeGetDTO>>> GetAllAsync(IncomeQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<Income, IncomeGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<IncomeGetDTO>>(
                true,
                "Incomes retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<IncomeGetDTO>>> GetDeletedAsync(IncomeQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<Income, IncomeGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<IncomeGetDTO>>(
                true,
                "Deleted incomes retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<IncomeGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.Incomes
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<IncomeGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<IncomeGetDTO>(
                    false,
                    "Income not found.",
                    null
                );
            }

            return new ApiResponse<IncomeGetDTO>(
                true,
                "Income retrieved successfully.",
                dto
            );
        }
        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse(
                    false,
                    "Income not found."
                );
            }

            var result = _context.Incomes.Remove(income);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Income could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Income could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Income deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse(
                    false,
                    "Income not found."
                );
            }

            income.IsDeleted = !income.IsDeleted;
            income.DeletedAt = income.IsDeleted ? DateTime.UtcNow : null;

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a parent restore resurrect this row.
            income.DeletedByCascade = false;

            var result = _context.Incomes.Update(income);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Income status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Income status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Income status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, IncomeUpdateDTO dto)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse(
                    false,
                    "Income not found."
                );
            }

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId, dto.DriverId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            _mapper.Map(dto, income);

            income.UpdatedAt = DateTime.UtcNow;

            var result = _context.Incomes.Update(income);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Income could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Income could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Income updated successfully."
            );
        }
    }
}
