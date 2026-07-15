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

        public IncomeService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse<object>> CreateAsync(IncomeCreateDTO dto)
        {
            Income income = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                DriverId = dto.DriverId,
                Amount = dto.Amount,
                IncomeDate = dto.IncomeDate,
                Description = dto.Description,
            };

            var result = await _context.Incomes.AddAsync(income);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Income could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Income could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Income created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<IncomeGetDTO>>> GetAllAsync()
        {
            var incomes = await _context.Incomes.ToListAsync();

            var dtos = incomes.Select(i => new IncomeGetDTO
            {
                Id = i.Id,

                VehicleId = i.VehicleId,
                DriverId = i.DriverId,
                Amount = i.Amount,
                IncomeDate = i.IncomeDate,
                Description = i.Description,

                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                DeletedAt = i.DeletedAt,
                IsDeleted = i.IsDeleted

            }).ToList();

            return new ApiResponse<List<IncomeGetDTO>>(
                true,
                "Incomes retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<IncomeGetDTO>> GetAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse<IncomeGetDTO>(
                    false,
                    "Income not found.",
                    null
                );
            }

            var dto = new IncomeGetDTO
            {
                Id = income.Id,

                VehicleId = income.VehicleId,
                DriverId = income.DriverId,
                Amount = income.Amount,
                IncomeDate = income.IncomeDate,
                Description = income.Description,

                CreatedAt = income.CreatedAt,
                UpdatedAt = income.UpdatedAt,
                DeletedAt = income.DeletedAt,
                IsDeleted = income.IsDeleted
            };

            return new ApiResponse<IncomeGetDTO>(
                true,
                "Income retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Income not found.",
                    null
                );
            }

            var result = _context.Incomes.Remove(income);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Income could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Income could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Income deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Income not found.",
                    null
                );
            }

            income.IsDeleted = !income.IsDeleted;
            income.DeletedAt = income.IsDeleted ? DateTime.Now : null;

            var result = _context.Incomes.Update(income);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Income status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Income status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Income status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, IncomeUpdateDTO dto)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Income not found.",
                    null
                );
            }

            income.VehicleId = dto.VehicleId ?? income.VehicleId;
            income.DriverId = dto.DriverId ?? income.DriverId;
            income.Amount = dto.Amount ?? income.Amount;
            income.IncomeDate = dto.IncomeDate ?? income.IncomeDate;
            income.Description = dto.Description ?? income.Description;

            income.UpdatedAt = DateTime.Now;

            var result = _context.Incomes.Update(income);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Income could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Income could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Income updated successfully.",
                null
            );
        }
    }
}
