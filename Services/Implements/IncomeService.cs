using AutoMapper;
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
            Income income = _mapper.Map<Income>(dto);

            income.CreatedAt = DateTime.Now;

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
            var incomes = await _context.Incomes
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<IncomeGetDTO>>(incomes);

            return new ApiResponse<List<IncomeGetDTO>>(
                true,
                "Incomes retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<IncomeGetDTO>>> GetDeletedAsync()
        {
            var incomes = await _context.Incomes
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<IncomeGetDTO>>(incomes);

            return new ApiResponse<List<IncomeGetDTO>>(
                true,
                "Deleted incomes retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<IncomeGetDTO>> GetAsync(Guid id)
        {
            var income = await _context.Incomes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (income == null)
            {
                return new ApiResponse<IncomeGetDTO>(
                    false,
                    "Income not found.",
                    null
                );
            }

            var dto = _mapper.Map<IncomeGetDTO>(income);

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
            income.DeletedAt = income.IsDeleted ? DateTime.Now : null;

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

            _mapper.Map(dto, income);

            income.UpdatedAt = DateTime.Now;

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
