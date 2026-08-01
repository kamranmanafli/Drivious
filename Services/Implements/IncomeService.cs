using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Income;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ApiResponse<List<IncomeGetDTO>>> GetAllAsync()
        {
            // Projected in the database so the vehicle and driver columns the DTO
            // carries are resolved by a join; mapping loaded entities leaves them null.
            var dtos = await _context.Incomes
                .Where(x => !x.IsDeleted)
                .ProjectTo<IncomeGetDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new ApiResponse<List<IncomeGetDTO>>(
                true,
                "Incomes retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<IncomeGetDTO>>> GetDeletedAsync()
        {
            var dtos = await _context.Incomes
                .Where(x => x.IsDeleted)
                .ProjectTo<IncomeGetDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new ApiResponse<List<IncomeGetDTO>>(
                true,
                "Deleted incomes retrieved successfully.",
                dtos
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
